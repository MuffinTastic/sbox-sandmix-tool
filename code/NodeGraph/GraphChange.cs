using SandMix.Nodes;

namespace SandMixTool.NodeGraph;

public struct GraphChange
{
	public bool Creation { get; set; } // false means deletion, naturally
	public Graph Graph { get; set; }
}
