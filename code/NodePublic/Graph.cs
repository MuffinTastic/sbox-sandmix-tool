using System.Collections.Generic;
using System.Linq;

namespace SandMixTool.NodeEditor;

public class Graph
{
	public List<BaseNode> Nodes { get; init; } = new();
	public List<(string, string)> Connections { get; init; } = new();

	public Graph()
	{
		Nodes = new List<BaseNode>();
	}

	public void Add( BaseNode node )
	{
		node.Graph = this;
		Nodes.Add( node );
	}

	public BaseNode Find( string name )
	{
		return Nodes.FirstOrDefault( x => x.IsNamed( name ) );
	}

	public void Connect( string from, string to )
	{
		Connections.Add( (from, to) );
	}
}
