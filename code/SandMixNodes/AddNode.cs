using SandMixTool.NodeEditor;

namespace SandMixTool.Nodes;

public class AdditionNode : BaseNode
{
	[Input]
	public float X { get; set; }

	[Input]
	public float Y { get; set; }

	[Output]
	public float Result => X + Y;
}
