using Tools;
using SandMix.Nodes;

namespace SandMix.Tool.NodeGraph;

public class PlugIn : PlugUI
{
	PlugIn DropTarget => Node.Graph.DropTarget;

	public BaseNode.InputAttribute Attribute;

	public PlugIn( NodeUI node, System.Reflection.PropertyInfo property, BaseNode.InputAttribute attribute ) : base( node, property )
	{
		Attribute = attribute;
	}

	public override Vector2 HandlePosition => new Vector2( 0, (Size.y - HandleSize) * 0.5f );

	protected override void OnPaint()
	{
		var isTarget = DropTarget == this;

		var spacex = 7;

		var color = HandleConfig.Color;

		if ( !isTarget )
		{
			color = color.Desaturate( 0.2f ).Darken( 0.3f );
		}

		var handleRect = new Rect( HandlePosition.x, HandlePosition.y, HandleSize, HandleSize );
		DrawHandle( color, handleRect );

		Paint.SetDefaultFont();
		Paint.SetPen( color );
		Paint.DrawText( new Rect( HandleSize + spacex, 0, Size.x - 4 - 10, Size.y ), Title, TextFlag.LeftCenter );
	}

}
