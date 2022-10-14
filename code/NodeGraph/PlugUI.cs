using System.Reflection;
using Tools;

namespace SandMix.Tool.NodeGraph;

public class PlugUI : GraphicsItem
{
	public const float HandleSize = 14;

	public NodeUI Node { get; protected set; }

	public string Title = "Unnamed Title";
	public PropertyInfo Property;

	public string Identifier => $"{Node.Node.Identifier}.{Property.Name}";

	public HandleConfig HandleConfig;

#pragma warning disable CS0114
	public virtual Vector2 HandlePosition => 0;
#pragma warning restore CS0114
	public virtual Vector2 HandleCenter => HandlePosition + HandleSize / 2;

	public PlugUI( NodeUI node, PropertyInfo property )
	{
		Size = new Vector2( 24, 24 );
		Node = node;
		Property = property;

		var display = Sandbox.DisplayInfo.ForMember( property );
		Title = display.Name;

		HandleConfig = node.Graph.GetHandleConfig( property.PropertyType );
	}

	protected void DrawHandle( in Color color, in Rect handleRect )
	{
		Paint.SetPenEmpty();
		Paint.SetBrush( HandleConfig.Color.WithAlpha( 1.0f ) );
		Paint.DrawRect( handleRect, 2.0f );

		Paint.SetPen( HandleConfig.Color.Darken( 0.5f ) );
		Paint.SetDefaultFont( 7, 500 );
		Paint.DrawText( handleRect, HandleConfig.Icon );
	}

	public bool IsNamed( string name )
	{
		return Property.Name == name;
	}
}

public struct HandleConfig
{
	public string Name { get; set; }
	public Color Color { get; set; }
	public string Icon { get; set; }
}
