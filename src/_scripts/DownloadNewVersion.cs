using Godot;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;

using FileAccess = Godot.FileAccess;

namespace yyvd;

public partial class DownloadNewVersion : Node
{
	private Global global;
	[Export]
	private CheckYoutubeDownload checkYoutubeDownload;
	private HttpRequest downloadRequest;

	[ExportCategory("Downloading New Version UI")]
	[Export]
	private Control gettingNewVersionPanel;
	[Export]
	private Control downloadingNewVersionPanel;
	[Export]
	private  ProgressBar downloadProgessBar;

	[ExportCategory("Download Error UI")]
	[Export]
	private Control downloadErrorPopup;
	[Export]
	private Button denyRetryDownloadButton;

	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
	}

	public void InitiateDownload()
	{
		downloadRequest = new()
		{
			DownloadFile = global.TempProgramPath,
			Timeout = 30,
		};
		AddChild(downloadRequest);
		downloadRequest.RequestCompleted += DownloadRequestCompleted;
		GD.Print("Newest version: " + Global.newestVersion);
		if(Global.newestVersion == "")
		{
			WaitCheckVersion();
		}
		else
		{
			Download();
		}
	}

	private async Task WaitCheckVersion()
	{
		gettingNewVersionPanel.Visible = true;
		checkYoutubeDownload.CheckNewVersion();
		while(true)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if(Global.newestVersion != "")
			{
				break;
			}
		}
		gettingNewVersionPanel.Visible = false;
		Download();
	}

	private void Download()
	{
		downloadProgessBar.Value = 0;
		downloadingNewVersionPanel.Show();
		var downloadUrl = Global.downloadUrl + Global.newestVersion + "/yt-dlp.exe";
		// var downloadUrl = "https://google.com";
		var error = downloadRequest.Request(downloadUrl);
		if(error != Error.Ok)
		{
			downloadRequest.CancelRequest();
			RemoveChild(downloadRequest);
			downloadRequest = null;
			ShowPopup();
		}
		WaitDownload();
	}
	private async Task ShowPopup()
	{
		downloadErrorPopup.Visible = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		denyRetryDownloadButton.Disabled = Global.currentVersion == "";
	}

	private async Task WaitDownload()
	{
		while(downloadRequest != null)
		{
			var downloadPercentage = (float)downloadRequest.GetDownloadedBytes() / (float)downloadRequest.GetBodySize();
			downloadProgessBar.Value = downloadPercentage;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		downloadingNewVersionPanel.Hide();
	}

	private void DownloadRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		if(result != (long) HttpRequest.Result.Success)
		{
			downloadingNewVersionPanel.Hide();
			downloadErrorPopup.Show();
			denyRetryDownloadButton.Disabled = Global.currentVersion == "";
			CleanupFailedDownload();
		}
		else
		{
			SaveNewVersion();
		}

		RemoveChild(downloadRequest);
		downloadRequest.QueueFree();
		downloadRequest = null;
	}

	private void CleanupFailedDownload()
	{
		if(FileAccess.FileExists(global.TempProgramPath))
		{
			var err = DirAccess.RemoveAbsolute(global.TempProgramPath);
			if(err != Error.Ok)
			{
				GD.PrintErr("ERROR DELETING TEMP DOWNLOAD");
			}
		}
	}

	private void SaveNewVersion()
	{
		Global.currentVersion = Global.newestVersion;
		
		using var file = FileAccess.Open(global.SaveFilePath, FileAccess.ModeFlags.Write);
		file.StoreString(Global.currentVersion);

		DirAccess.RenameAbsolute(global.TempProgramPath, global.ProgramPath);
	}
}
