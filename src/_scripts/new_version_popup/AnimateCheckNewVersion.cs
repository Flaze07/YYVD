using Godot;
using System;
using System.Threading.Tasks;

public partial class AnimateCheckNewVersion : Label
{
	[Export]
	private string baseText = "";
	[Export]
	private float animationDelay;
	private int dotCount = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Global.programState = "Checking New Version";
		AnimateText();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private async Task AnimateText()
	{
		while(true)
		{
			if(dotCount > 3)
			{
				dotCount = 0;
			}

			string newText = baseText + new string('.', dotCount);
			this.Text = newText;

			if(Global.programState != "Checking New Version")
			{
				return;
			}

			dotCount += 1;

			await ToSignal(GetTree().CreateTimer(animationDelay), Timer.SignalName.Timeout);
		}
	}
}
