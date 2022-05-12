using Tools;

namespace SandMixTool.NodeEditor;

public class Connection : Tools.GraphicsLine
{
	public PlugOut Output { get; protected set; }
	public PlugIn Input { get; protected set; }

	Vector2 PreviewPosition;

	public Connection( PlugOut output, PlugIn input ) : this( output )
	{
		HoverEvents = true;
		Selectable = true;
		Input = input;
		Cursor = CursorShape.Finger;
	}

	// preview
	public Connection( PlugOut output )
	{
		Output = output;
		ZIndex = -10;
	}

	protected override void OnPaint()
	{
		if ( Output == null ) return;
		if ( Dragging ) return;

		var color = Output.HandleConfig.Color;
		var width = 4.0f;

		if ( Input == null ) color = Output.HandleConfig.Color.WithAlpha( 0.4f );

		if ( Paint.HasSelected )
		{
			color = Output?.Node?.SelectionOutline.Darken( 0.2f ) ?? color;
			width = 4.0f;
		}

		if ( Paint.HasMouseOver )
		{
			color = Output?.Node?.SelectionOutline ?? color;
			width = 6.0f;
		}


		Paint.SetPen( color, width );

		PaintLine();
	}

	internal void LayoutForPreview( NodeEditor.PlugOut nodeOutput, Vector2 scenePosition, NodeEditor.PlugIn dropTarget )
	{
		Output = nodeOutput;
		Input = dropTarget;

		if ( Output != null && Input != null )
		{
			Layout();
			return;
		}

		var rect = new Rect( nodeOutput.HandlePosition );
		rect = rect.AddPoint( scenePosition );

		rect.Position -= 100.0f;
		rect.Size += 200.0f;

		Position = rect.Position;
		Size = rect.Size;

		Clear();

		PreviewPosition = FromScene( scenePosition );
		var legde = new Vector2( 20, 0 );

		var startPos = FromScene( Output.HandlePosition );

		MoveTo( startPos );
		startPos += legde;
		LineTo( startPos );

		var pp = FromScene( PreviewPosition );
		CubicLineTo( startPos + new Vector2( 100, 0 ), (startPos + PreviewPosition) * 0.5f, PreviewPosition );

		Update();
	}

	public void Layout()
	{
		var rect = new Rect( Output.HandlePosition );
		rect = rect.AddPoint( Input.HandlePosition );

		rect.Position -= 100.0f;
		rect.Size += 200.0f;

		Position = rect.Position;
		Size = rect.Size;

		Clear();

		var legde = new Vector2( 16, 0 );

		var startPos = FromScene( Output.HandlePosition );
		var endPos = FromScene( Input.HandlePosition ) - legde;

		MoveTo( startPos );
		startPos += legde;
		LineTo( startPos );
		CubicLineTo( startPos + new Vector2( 64, 0 ), endPos + new Vector2( -64, 0 ), endPos );
		endPos += legde;
		LineTo( endPos );

		Update();
	}

	internal bool IsAttachedTo( NodeUI node )
	{
		if ( node == Output?.Node ) return true;
		if ( node == Input?.Node ) return true;

		return false;
	}

	bool Dragging;

	protected override void OnMouseReleased( GraphicsMouseEvent e )
	{
		if ( Dragging )
		{
			Output.Node.DroppedOutput( Output, e.ScenePosition, this );
			if ( !IsValid ) return; // connection might get deleted here
		}

		Dragging = false;
		Cursor = CursorShape.Finger;
		Update();
	}

	protected override void OnMouseMove( GraphicsMouseEvent e )
	{
		Dragging = true;
		Cursor = CursorShape.DragLink;
		Output.Node.DraggingOutput( Output, e.ScenePosition, this );
		Update();
	}

	public void Disconnect()
	{
		Output.Node.Graph.RemoveConnection( this );
	}
}
