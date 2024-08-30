using Godot;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

public partial class DownloadNewestVersion : Node
{
	[Export]
	private Window downloadProgressWindow;
	[Export]
	private AcceptDialog failedDownloadDialog;
	[Signal]
	public delegate void DownloadProgressEventHandler(int maxValue, int currentProgress);
	private HttpRequest downloadRequest;

	private Global globalNode;
	private string filePath;
	private string programPath;
	private string tempProgramPath;
	private bool succeed;
    public override void _Ready()
    {
		globalNode = GetNode<Global>("/root/Global");
		filePath = globalNode.SaveFilePath;
		programPath = globalNode.ProgramPath;
		tempProgramPath = globalNode.TempProgramPath;
    }

	public void DownloadNewVersion(string downloadUrl, string downloadVersion)
	{
		downloadProgressWindow.Visible = true;
		succeed = false;
		ExecuteDownload(downloadUrl, downloadVersion);
	}

	public async Task ExecuteDownload(string downloadUrl, string downloadVersion)
	{
        downloadRequest = new()
        {
            DownloadFile = tempProgramPath
        };
		AddChild(downloadRequest);
        downloadRequest.RequestCompleted += DownloadRequestCompleted;
		var error = downloadRequest.Request(downloadUrl);

		if(error != Error.Ok)
		{
			GD.Print("ERROR");
			return;
		}

		while(true)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if(downloadRequest == null)
			{
				break;
			}
			EmitSignal(SignalName.DownloadProgress, downloadRequest.GetBodySize(), downloadRequest.GetDownloadedBytes());
		}

		if(succeed)
		{
			downloadProgressWindow.Visible = false;
			SaveDownloadedVersion(downloadVersion);
		}
	}

	public void SaveDownloadedVersion(string downloadVersion)
	{
		using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
		file.StoreString(downloadVersion);
	}

	private void DownloadRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		if(result != (long) HttpRequest.Result.Success)
		{
			GD.Print("Failed to download ytdl");
			downloadProgressWindow.Visible = false;
			failedDownloadDialog.Visible = true;
			if(FileAccess.FileExists(tempProgramPath))
			{
				var err = DirAccess.RemoveAbsolute(tempProgramPath);
				if(err != Error.Ok)
				{
					failedDownloadDialog.DialogText += "\nFAILED TO REMOVE TEMPORARY DOWNLOADED PROGRAM ( SHOULDN'T BE AN ISSUE)";
				}
			}
		}
		else
		{
			succeed = true;
			if(FileAccess.FileExists(programPath))
			{
				DirAccess.RemoveAbsolute(programPath);
			}
			DirAccess.RenameAbsolute(tempProgramPath, programPath);
		}
		RemoveChild(downloadRequest);
		downloadRequest = null;
	}
}
