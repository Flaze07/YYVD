using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public partial class MainUIHandler : Node
{
	#region Video Checking Error
	
	[Export]
	private Button checkingVideoErrorPopupButton;

	[Export]
	private Control checkingVideoErrorPopup;

	#endregion

	[Export]
	private Control newVersionPrompt;

	[Export]
	private Control downloadErrorPopup;

	[Export]
	private Node resolutionButtonParent;

	#region res buttons

	[Export]
	private PackedScene resolutionButton;

	[Export]
	private Control downloadingVideoPanel;

	[Export]
	private ProgressBar downloadProgressBar;

	[Export]
	private Control processingPanel;

	[Export]
	private Control downloadCompletedPanel;

	#endregion

	[Export]
	private TextureRect thumbnail;
	public TextureRect Thumbnail => thumbnail;

	[Export]
	private Label title;
	public Label Title => title;

	[Export]
	private LineEdit urlInput;
	public override void _Ready()
	{
		checkingVideoErrorPopupButton.Pressed += CheckingVideoErrorPopupButtonPressed;
	}

	private void CheckingVideoErrorPopupButtonPressed()
	{
		checkingVideoErrorPopup.Visible = false;
	}

	public void NewVersionPromptButtonPressed()
	{
		newVersionPrompt.Visible = false;
	}

	public void DownloadErrorPopupButtonPressed()
	{
		downloadErrorPopup.Visible = false;
	}

	public void AddResolutionButtons(string[] resList)
	{
		for(int i = 0; i < resolutionButtonParent.GetChildCount(); ++i)
		{
			resolutionButtonParent.GetChild(i).QueueFree();
		}

		GD.Print(resList);

		foreach(var res in resList)
		{
			string[] resType = res.Split("x");

			// Button btn = (Button) resolutionButton.Instantiate();
			var btn = resolutionButton.Instantiate<DownloadButton>();
			resolutionButtonParent.AddChild(btn);
			var videoHeight = resType[1];
			btn.Initialize(downloadingVideoPanel, downloadProgressBar, processingPanel, downloadCompletedPanel);
			btn.Text = videoHeight;
			btn.DownloadType = videoHeight;
			btn.VideoUrl = urlInput.Text;
		}
	}
}
