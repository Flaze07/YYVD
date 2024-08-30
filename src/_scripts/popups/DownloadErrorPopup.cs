using Godot;
using System;

public partial class DownloadErrorPopup : Node
{
	[Export]
	private Control downloadErrorPopup;
	[Export]
	private Button denyButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		downloadErrorPopup.VisibilityChanged += popupVisibilityChanged;
	}

	private void popupVisibilityChanged()
	{
		denyButton.Disabled = false;
	}
}
