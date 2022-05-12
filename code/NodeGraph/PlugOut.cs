using Tools;

namespace SandMixTool.NodeEditor;

public class PlugOut : Plug
{
	public bool Dragging { get; protected set; }

	public BaseNode.OutputAttribute Attribute;

	public PlugOut( NodeUI node, System.Reflection.PropertyInfo property, BaseNode.OutputAttribute attribute ) : base( node, property )
	{
		HoverEvents = true;
		Attribute = attribute;
		Cursor = CursorShape.Finger;
	}

	protected override void OnPaint()
	{
		var highlight = Paint.HasMouseOver;

		var rect = new Rect();
		rect.Size = Size;

		var spacex = 7;


		var color = HandleConfig.Color;

		if ( !highlight )
		{
			color = color.Desaturate( 0.2f ).Darken( 0.3f );
		}

		var handleRect = new Rect( rect.width - handleSize, (rect.height - handleSize) * 0.5f, handleSize, handleSize );
		DrawHandle( color, handleRect );

		Paint.SetDefaultFont();
		Paint.SetPen( color );
		Paint.DrawText( new Rect( spacex, 0, rect.width - handleSize - spacex * 2, rect.Size.y ), Title, TextFlag.RightCenter );
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
