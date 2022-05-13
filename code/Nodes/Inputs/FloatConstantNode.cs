using System.ComponentModel.DataAnnotations;
using Sandbox;
using SandMixTool.NodeGraph;

namespace SandMixTool.Nodes.Inputs;

[Library, Display( Name = "Constant Float", Description = "Constant float input", GroupName = "Inputs" )]
public class FloatConstantNode : BaseNode
{
	[Constant, Output]
	public float Value { get; set; } = 0;
}
