using Godot;
using Godot.Collections;
using System;

using Array = Godot.Collections.Array;

namespace yyvd;

public partial class RuntimeRemoveNode: Node
{
	[Export]
	private Array<Node> nodes;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach(var node in nodes)
		{
			node.QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
