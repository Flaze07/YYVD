using Godot;
using System;
using System.Net.NetworkInformation;

namespace yyvd;

public partial class CheckVersionEventHandler : Node
{
	[Export]
	private Control loadingPanel;

	#region Error Controls
	[ExportCategory("Error Controls")]	
	[Export]
	private Control errorPopup;
	[Export]
	private Label errorText;

	#endregion

	#region Complete Controls
	[ExportCategory("Complete Controls")]
	[Export]
	private Control newVersionPrompt;
	[Export]
	private Button noButton;

	#endregion

	public void CheckingError(string errorMessage)
	{
		loadingPanel.Hide();
		errorPopup.Show();
		errorText.Text = errorMessage;
	}

	public void CheckingComplete(bool newest)
	{
		if(!newest)
		{
			loadingPanel.Hide();
			newVersionPrompt.Show();
			noButton.Disabled = Global.currentVersion == "";
		}
		else
		{
			loadingPanel.Hide();
		}
	}
}