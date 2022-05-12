using System.ComponentModel.DataAnnotations;
using SandMixTool.NodeEditor;

namespace SandMixTool.Nodes;

[Display( Name = "Float Input", GroupName = "Generic" )]
public class FloatNode : BaseNode
{
	[Input]
	public float Float { get; set; }

	[Output, Display( Name = "Out" )]
	public float FloatOut => Float;
}
