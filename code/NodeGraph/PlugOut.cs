using Tools;
using SandMix.Nodes;
using Sandbox;

namespace SandMix.Tool.NodeGraph;

public class PlugOut : PlugUI
{
	public bool Dragging { get; protected set; }

	public BaseNode.OutputAttribute Attribute;

	public PlugOut( NodeUI node, System.Reflection.PropertyInfo property, BaseNode.OutputAttribute attribute ) : base( node, property )
	{
		HoverEvents = true;
		Attribute = attribute;
		Cursor = CursorShape.Finger;
	}

	public override Vector2 HandlePosition => new Vector2( Size.x - HandleSize, (Size.y - HandleSize) * 0.5f );


	protected override void OnPaint()
	{
		var highlight = Paint.HasMouseOver;

		var spacex = 7;

		var color = HandleConfig.Color;

		if ( !highlight )
		{
			color = color.Desaturate( 0.2f ).Darken( 0.3f );
		}

		var handleRect = new Rect( HandlePosition.x, HandlePosition.y, HandleSize, HandleSize );
		DrawHandle( color, handleRect );

		Paint.SetDefaultFont();
		Paint.SetPen( color );
		Paint.DrawText( new Rect( spacex, 0, Size.x - HandleSize - spacex * 2, Size.y ), Title, TextFlag.RightCenter );
	}


	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		base.OnMousePressed( e );

		if ( e.LeftMouseButton )
		{
			e.Accepted = true;
		}
	}

	protected override void OnMouseReleased( GraphicsMouseEvent e )
	{
		if ( Dragging )
		{
			Node.DroppedOutput( this, e.ScenePosition );
		}

		Dragging = false;
		Cursor = CursorShape.Finger;
		Update();
	}

	protected override void OnMouseMove( GraphicsMouseEvent e )
	{
		// TODO - minimum distance move

		Dragging = true;
		Node.DraggingOutput( this, e.ScenePosition );
		Cursor = CursorShape.DragLink;
		Update();
		e.Accepted = true;
	}
}
