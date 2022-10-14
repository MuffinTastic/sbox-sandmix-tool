using SandMix.Nodes;

namespace SandMix.Tool.NodeGraph;

public struct GraphChange
{
	public bool Creation { get; set; } // false means deletion, naturally
	public GraphContainer Graph { get; set; }
}
