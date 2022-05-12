using Tools;

namespace SandMixTool.NodeEditor;

public class Plug : Tools.GraphicsItem
{
	protected const float handleSize = 14;

	public NodeUI Node { get; protected set; }

	public string Title = "Unnammed Title";
	public System.Reflection.PropertyInfo Property;



	public HandleConfig HandleConfig;

	public Plug( NodeUI node, System.Reflection.PropertyInfo property )
	{
		Size = new Vector2( 24, 24 );
		Node = node;
		Property = property;

		var display = Sandbox.DisplayInfo.ForProperty( property );
		Title = display.Name;

		HandleConfig = node.Graph.GetHandleConfig( property.PropertyType );
	}

	protected void DrawHandle( in Color color, in Rect handleRect )
	{
		Paint.SetPenEmpty();
		Paint.SetBrush( HandleConfig.Color.WithAlpha( 1.0f ) );
		Paint.DrawRect( handleRect, 2.0f );

		Paint.SetPen( HandleConfig.Color.Darken( 0.25f ) );
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
