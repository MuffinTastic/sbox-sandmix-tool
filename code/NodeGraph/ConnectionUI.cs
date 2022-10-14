using Tools;

namespace SandMix.Tool.NodeGraph;

public class ConnectionUI : GraphicsLine
{
	public PlugOut Output { get; protected set; }
	public PlugIn Input { get; protected set; }

	Vector2 PreviewPosition;

	public ConnectionUI( PlugOut output, PlugIn input ) : this( output )
	{
		HoverEvents = true;
		Selectable = true;
		Input = input;
		Cursor = CursorShape.Finger;
	}

	// preview
	public ConnectionUI( PlugOut output )
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

	internal void LayoutForPreview( NodeGraph.PlugOut nodeOutput, Vector2 scenePosition, NodeGraph.PlugIn dropTarget )
	{
		Output = nodeOutput;
		Input = dropTarget;

		var OutputType = Output?.Property.PropertyType;
		var InputType = Input?.Property.PropertyType;

		if ( Output != null && Input != null && OutputType == InputType )
		{
			Layout();
			return;
		}

		var rect = new Rect( nodeOutput.Node.Position + nodeOutput.Position + nodeOutput.HandleCenter );
		rect = rect.AddPoint( scenePosition );

		rect.Position -= 100.0f;
		rect.Size += 200.0f;

		Position = rect.Position;
		Size = rect.Size;

		Clear();

		PreviewPosition = FromScene( scenePosition );
		var legde = new Vector2( 20, 0 );

		var startPos = FromScene( Output.Node.Position + Output.Position + Output.HandleCenter );

		MoveTo( startPos );
		startPos += legde;
		LineTo( startPos );

		var pp = FromScene( PreviewPosition );
		CubicLineTo( startPos + new Vector2( 100, 0 ), (startPos + PreviewPosition) * 0.5f, PreviewPosition );

		Update();
	}

	public void Layout()
	{
		var rect = new Rect( Output.Node.Position + Output.Position + Output.HandleCenter );
		rect = rect.AddPoint( Input.Node.Position + Input.Position + Input.HandleCenter );

		rect.Position -= 100.0f;
		rect.Size += 200.0f;

		Position = rect.Position;
		Size = rect.Size;

		Clear();

		var legde = new Vector2( 16, 0 );

		var startPos = FromScene( Output.Node.Position + Output.Position + Output.HandleCenter );
		var endPos = FromScene( Input.Node.Position + Input.Position + Input.HandleCenter ) - legde;

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
