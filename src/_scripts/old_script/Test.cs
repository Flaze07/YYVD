using Godot;
using System;
using System.Threading.Tasks;

using HttpRequest = Godot.HttpRequest;

public partial class Test : Node
{
	[Export]
	private TextEdit output;

	private HttpRequest checkVersionRequest;
	private HttpRequest downloadRequest;

	private bool newVersion = false;
	private bool downloadingNewVersion = false;
	private string downloadUrl;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// checkVersionRequest = new();
		// AddChild(checkVersionRequest);
		// checkVersionRequest.RequestCompleted += CheckVersionRequestCompleted;

		// Error err = checkVersionRequest.Request("https://github.com/yt-dlp/yt-dlp/releases");

		// if(err != Error.Ok)
		// {
		// 	GD.PushError("An error occured in the HTTP Requests");
		// }

		Temp();
		GD.Print("This goes first");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		// if(newVersion)
		// {
		// 	if(!downloadingNewVersion)
		// 	{
		// 		downloadRequest = new();
		// 		AddChild(downloadRequest);
		// 		downloadRequest.DownloadFile = "user://program.exe";
		// 		downloadRequest.RequestCompleted += DownloadRequestCompleted;
		// 		Error err = downloadRequest.Request(downloadUrl);
		// 		if(err != Error.Ok)
		// 		{
		// 			GD.PushError("Errored");
		// 			return;
		// 		}
		// 		downloadingNewVersion = true;
		// 		output.Text = "";
		// 	}
		// 	else
		// 	{
		// 		output.Text = downloadRequest.GetBodySize().ToString();
		// 		output.Text += "\n" + downloadRequest.GetDownloadedBytes();
		// 	}
		// }
		GD.Print("PROCESS");
	}

	private async Task Temp()
	{
		await ToSignal(GetTree().CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);
		int idx = 0;
		while(true)
		{
			GD.Print(idx);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			idx += 1;
		}
	}

	private void CheckVersionRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		/**
		 * Once we obtained the HTML,
		 * we will look up the css class Link--primary
		 * Link--primary always refer to the versions
		 * getting the first one means getting the latest version
		 * once we got the location of the css, we will get the href by iterating backward
		 * and then we will find the location of two quotes which makes up the link in href
		 */

		var str = System.Text.Encoding.Default.GetString(body);
		output.Text = str;
		int classIdx = str.Find("Link--primary");
		int hrefIdx = str.LastIndexOf("href", classIdx);
		output.Text += hrefIdx;
		int firstQuoteIdx = str.Find("\"", hrefIdx);
		int secondQuoteIdx = str.Find("\"", firstQuoteIdx + 1);

		var substr = str.Substring(firstQuoteIdx + 1, secondQuoteIdx - firstQuoteIdx - 1);
		downloadUrl = "https://github.com" + substr + "/yt-dlp.exe";
		downloadUrl = downloadUrl.Replace("tag", "download");
		// output.Text += "https://github.com" + substr + "/yt-dlp.exe";
		GD.Print(downloadUrl);

		newVersion = true;

		RemoveChild(checkVersionRequest);
		checkVersionRequest = null;
	}

	private void DownloadRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		RemoveChild(downloadRequest);
		downloadRequest = null;
		newVersion = false;
		downloadingNewVersion = false;
	}
}
