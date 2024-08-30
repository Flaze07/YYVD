using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class DownloadButton : Button 
{
	private Global global;

	#region UIs
	
	private Control downloadPanel;
	private ProgressBar downloadProgressBar;
	private Control processingPanel;
	private Control downloadCompletedPanel;

	#endregion

	private string videoUrl;
	public string VideoUrl { get => videoUrl; set => videoUrl = value; }
	private string downloadType;
	public string DownloadType { get => downloadType; set => downloadType = value; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		global = GetNode<Global>("/root/Global");
		this.Pressed += OnDownloadClicked;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Initialize(Control downloadPanel, ProgressBar downloadProgressBar, Control processingPanel, Control downloadCompletedPanel)
	{
		this.downloadPanel = downloadPanel;
		this.downloadProgressBar = downloadProgressBar;
		this.processingPanel = processingPanel;
		this.downloadCompletedPanel = downloadCompletedPanel;
	}

	private void OnDownloadClicked()
	{
		if(downloadType == "audio")
		{
			return;
		}

		var workingDir = ProjectSettings.GlobalizePath(global.BasePath);

		var url = videoUrl;
		var arguments = $"-f 'bv*[height={downloadType}]+ba -P {workingDir} {url}";
	
		var downloadingVideo = false;
		var videoDownloaded = false;
		void outputHandler(string output)
		{
			if(output.Contains("has already been downloaded"))
			{
				downloadPanel.CallDeferred(Control.MethodName.SetVisible, false);
				downloadProgressBar.CallDeferred(ProgressBar.MethodName.SetValue, 1);
			}
			if(downloadingVideo)
			{
				/**
				 * check for deestination because there's 2 downloads
				 * video and audio
				 * for each download, yt-dlp will show destination first
				 */
				if(output.Contains("Destination"))
				{
					videoDownloaded = true;
					downloadingVideo = false;
					downloadPanel.CallDeferred(Control.MethodName.SetVisible, false);
					processingPanel.CallDeferred(Control.MethodName.SetVisible, true);
				}

				var splitted = output.Split(" ").Where(x => x != "").ToArray();
				var percentageStr = splitted[1];
				if(percentageStr.Length > 0)
				{
					percentageStr = percentageStr.Remove(percentageStr.Length - 1);
				}
                if(float.TryParse(percentageStr, out float percentage))
                {
					downloadProgressBar.CallDeferred(ProgressBar.MethodName.SetValue, percentage / 100.0f);
                }
            }

			if(output.StartsWith("[download]") && !videoDownloaded)
			{
				downloadingVideo = true;
			}
		}

		void completedHandler()
		{
			// proceessingPanel.Visible = false;
			processingPanel.CallDeferred(Control.MethodName.SetVisible, false);
			_ = ShowCompletedDownload();
		}

		downloadPanel.Visible = true;
		downloadProgressBar.Value = 0;

		_ = global.RunCommand(global.ProgramPath, arguments, outputHandler, completedHandler);
	}

	private async Task ShowCompletedDownload()
	{
		downloadCompletedPanel.CallDeferred(Control.MethodName.SetVisible, true);
		await ToSignal(GetTree().CreateTimer(3), "timeout");
		downloadCompletedPanel.CallDeferred(Control.MethodName.SetVisible, false);
	}
}
