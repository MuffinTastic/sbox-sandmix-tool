using SandMixTool.NodeGraph;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SandMixTool.Nodes.Operations;

[Display( Name = "Sub Float", Description = "Subtract one float from another", GroupName = "Operations" )]
public class FloatSubNode : BaseNode
{
	[Input]
	public float X { get; set; }

	[Input]
	public float Y { get; set; }

	[Browsable( false ), Output]
	public float Result => X - Y;
}
