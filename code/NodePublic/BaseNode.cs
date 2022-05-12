using System.ComponentModel;

namespace SandMixTool.NodeEditor;

public abstract class BaseNode
{
	[Browsable( false )]
	public string Identifier { get; set; }

	[Browsable( false )]
	public Vector2 Position { get; set; }

	[Browsable( false )]
	public Graph Graph { get; set; }

	public BaseNode()
	{
		Identifier = System.Guid.NewGuid().ToString();
	}

	public virtual string GetTitle()
	{
		return null;
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

}
