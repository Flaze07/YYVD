using Godot;
using Godot.Collections;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

public partial class CheckNewestVersion : Node
{
	[Signal]
	public delegate void DownloadNewVersionEventHandler(string downloadUrl, string versionDownloaded);
	private Global globalNode;
	private string filePath;
	private bool newestVersion = true;
	private HttpRequest checkVersionRequest;
	private string currentVersion = "";
	private string downloadUrl = "";
	private string downloadVersion = "";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		globalNode = GetNode<Global>("/root/Global");
		filePath = globalNode.SaveFilePath;
		CheckFile();
		_ = CheckLatestVersion();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CheckFile()
	{
		if(!FileAccess.FileExists(filePath))
		{
			return;
		}

		using var f = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		if(f == null)
		{
			GD.Print(f.GetError());
		}
		currentVersion = f.GetAsText(true);
		GD.Print("CurrentVersion: " + currentVersion);
	}

	private async Task CheckLatestVersion()
	{
		checkVersionRequest = new();
		AddChild(checkVersionRequest);
		checkVersionRequest.RequestCompleted += CheckVersionRequestCompleted;
		checkVersionRequest.Request("https://github.com/yt-dlp/yt-dlp/releases");

		while(true)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if(checkVersionRequest == null) 
			{
				break;
			}
		}

		if(!newestVersion)
		{
			EmitSignal(SignalName.DownloadNewVersion, downloadUrl, downloadVersion);
		}
	}

	private void CheckVersionRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		var str = System.Text.Encoding.Default.GetString(body);
		int classIdx = str.Find("Link--primary");
		int hrefIdx = str.LastIndexOf("href", classIdx);
		int firstQuoteIdx = str.Find("\"", hrefIdx);
		int secondQuoteIdx = str.Find("\"", firstQuoteIdx + 1);

		var linkSubstr = str.Substring(firstQuoteIdx + 1, secondQuoteIdx - firstQuoteIdx - 1);
		var tagIdx = linkSubstr.Find("tag/");
		var checkedVersion = linkSubstr.Substring(tagIdx + 4);

		newestVersion = currentVersion == checkedVersion;

		if(!newestVersion)
		{
			downloadUrl = "https://github.com" + linkSubstr + "/yt-dlp.exe";
			downloadUrl = downloadUrl.Replace("tag", "download");
			downloadVersion = checkedVersion;
		}

		RemoveChild(checkVersionRequest);
		checkVersionRequest = null;
	}
}
