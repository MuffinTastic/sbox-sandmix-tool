using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SandMixTool.NodeGraph;

public abstract class BaseNode
{
	[Browsable( false )]
	public string Identifier { get; set; }

	[Browsable( false )]
	public Vector2 Position { get; set; }

	[Browsable( false ), JsonIgnore]
	public Graph Graph { get; set; }

	public string Name { get; set; }
	public string Comment { get; set; }

	public BaseNode()
	{
		Identifier = System.Guid.NewGuid().ToString();
	}

	public bool IsNamed( string name )
	{
		return string.Equals( name, Identifier, System.StringComparison.OrdinalIgnoreCase );
	}

	public class InputAttribute : System.Attribute
	{

	}

	public class OutputAttribute : System.Attribute
	{

	}

	public class ConstantAttribute : System.Attribute
	{

	}
}
