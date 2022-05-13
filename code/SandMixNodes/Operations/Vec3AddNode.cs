﻿using SandMixTool.NodeGraph;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SandMixTool.Nodes.Operations;

[Display( Name = "Add Vector3", Description = "Add two Vector3s together", GroupName = "Operations" )]
public class Vec3AddNode : BaseNode
{
	[Input]
	public Vector3 X { get; set; }

	[Input]
	public Vector3 Y { get; set; }

	[Browsable( false ), Output]
	public Vector3 Result => X + Y;
}
