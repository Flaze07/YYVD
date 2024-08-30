using Godot;
using System;
using System.Threading.Tasks;

namespace yyvd;

public partial class AnimateLabel : Label
{
	[Export]
	private string baseText = "";
	[Export]
	private float animationDelay;
	private int dotCount = 0;
	[Export]
	private Control parentControl;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parentControl.VisibilityChanged += () => 
		{
			if(parentControl.Visible)
			{
				_ = AnimateText();
			}
		};
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

			if(parentControl.Visible == false)
			{
				return;
			}

			dotCount += 1;

			await ToSignal(GetTree().CreateTimer(animationDelay), Timer.SignalName.Timeout);
		}
	}
}
