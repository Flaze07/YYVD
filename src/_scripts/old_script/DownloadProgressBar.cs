using Godot;
using System;

public partial class DownloadProgressBar : Node
{
	[Export]
	private ProgressBar downloadProgressBar;
	public void DownloadProgress(int maxValue, int currentProgress)
	{
		float progress = (float)currentProgress / (float)maxValue;
		downloadProgressBar.Value = progress;
	}
}
