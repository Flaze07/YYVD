using Godot;
using System;
using System.Threading.Tasks;

namespace yyvd;

public partial class CheckYoutubeDownload : Node
{
	[Signal]
	public delegate void CheckingErrorEventHandler(string errorMessage);

	[Signal]
	public delegate void CheckingCompleteEventHandler(bool newest);

	private Global globalNode;
	private string versionFilePath;
	private bool isNewestVersion = false;

	#region HTTPRequests
	
	private HttpRequest checkVersionRequest;

	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		globalNode = GetNode<Global>("/root/Global");

		versionFilePath = globalNode.SaveFilePath;

		CallDeferred(MethodName.Initialize);
	}


	private void Initialize()
	{
		if(!CheckFile())
		{
			EmitSignal(SignalName.CheckingComplete, isNewestVersion);
			return;
		}
		if(!GetCurrentVersion())
		{
			return;
		}
		CheckNewVersionEarly();
	}

	private bool CheckFile()
	{
		return FileAccess.FileExists(versionFilePath);
	}

	//this function's returns indicates whether or not file access succeed
	private bool GetCurrentVersion()
	{
		using var file = FileAccess.Open(versionFilePath, FileAccess.ModeFlags.Read);
		if(file == null)
		{
			EmitSignal(SignalName.CheckingError, "ERROR WHILE CHECKING CURRENT YOUTUBE-DL VERSION.\n PLEASE RESTART THE SOFTWARE");
			return false;
		}

		Global.currentVersion = file.GetAsText();
		return true;
	}

	public void CheckNewVersionEarly()
	{
		CheckNewVersion();
		HandleVersion();
	}

	public void CheckNewVersion()
	{
		checkVersionRequest = new();
		AddChild(checkVersionRequest);
		checkVersionRequest.RequestCompleted += CheckVersionRequestCompleted;
		checkVersionRequest.Request(Global.checkVersionUrl);
	}

	private async Task HandleVersion()
	{
		while(true)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if(checkVersionRequest == null)
			{
				break;
			}
		}

		EmitSignal(SignalName.CheckingComplete, isNewestVersion);
	}

	private void CheckVersionRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		try
		{
			if(result != (long) HttpRequest.Result.Success)
			{
				throw new Exception("ERROR WHILE CHECKING FOR THE LATEST YOUTUBE-DL VERSION");
			}

			var str = System.Text.Encoding.Default.GetString(body);
			int classIdx = str.Find("Link--primary");
			int hrefIdx = str.LastIndexOf("href", classIdx);
			int firstQuoteIdx = str.Find("\"", hrefIdx);
			int secondQuoteIdx = str.Find("\"", firstQuoteIdx + 1);

			var linkSubstr = str.Substring(firstQuoteIdx + 1, secondQuoteIdx - firstQuoteIdx - 1);
			var tagIdx = linkSubstr.Find("tag/");
			var checkedVersion = linkSubstr.Substring(tagIdx + 4);

			Global.newestVersion = checkedVersion;

			isNewestVersion = Global.newestVersion == Global.currentVersion;
		} 
		catch(Exception e)
		{
			EmitSignal(SignalName.CheckingError, e.Message);
		} 
		finally
		{
			RemoveChild(checkVersionRequest);
			checkVersionRequest.QueueFree();
			checkVersionRequest = null;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
