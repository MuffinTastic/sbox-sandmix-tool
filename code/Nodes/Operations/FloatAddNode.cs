using Sandbox;
using SandMixTool.NodeGraph;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SandMixTool.Nodes.Operations;

[Library, Display( Name = "Add Float", Description = "Add two floats together", GroupName = "Operations" )]
public class FloatAddNode : BaseNode
{
	[Input]
	public float X { get; set; }

	[Input]
	public float Y { get; set; }

	[Browsable( false ), Output]
	public float Result => X + Y;
}
