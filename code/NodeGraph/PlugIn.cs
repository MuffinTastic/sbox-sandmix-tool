using Tools;

namespace SandMixTool.NodeEditor;

public class PlugIn : Plug
{
	PlugIn DropTarget => Node.Graph.DropTarget;

	public BaseNode.InputAttribute Attribute;

	public PlugIn( NodeUI node, System.Reflection.PropertyInfo property, BaseNode.InputAttribute attribute ) : base( node, property )
	{
		Attribute = attribute;
	}

	protected override void OnPaint()
	{
		var isTarget = DropTarget == this;

		var rect = new Rect();
		rect.Size = Size;

		var spacex = 7;
		var handleSize = 13;

		var color = HandleConfig.Color;

		if ( !isTarget )
		{
			color = color.Desaturate( 0.2f ).Darken( 0.3f );
		}

		var handleRect = new Rect( 0, (Size.y - handleSize) * 0.5f, handleSize, handleSize );
		DrawHandle( color, handleRect );

		Paint.SetDefaultFont();
		Paint.SetPen( color );
		Paint.DrawText( new Rect( handleSize + spacex, 0, rect.width - 4 - 10, rect.Size.y ), Title, TextFlag.LeftCenter );
	}

}
