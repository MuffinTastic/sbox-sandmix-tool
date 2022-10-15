using System;
using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

/// <summary>
/// A text box with a draggable icon on the left.
/// Dragging the icon left and right will change the value
/// Dragging the icon up and down will change the rate at which the value changes
/// So dragging down, then left and right will change in 100s, dragging up and then left and right will change in 0.01s
/// </summary>
[SandMixInspector( typeof( int ), "int" )]
public class IntProperty : Widget
{
	public string Label { get; set; }
	public string Icon { get; set; } = "edit";
	public Color HighlightColor = Color.Yellow;

	public event Action OnValueEdited;

	public int Value
	{
		get => LineEdit.Text.ToInt();

		set
		{
			LineEdit.Text = value.ToString();
		}
	}

	public LineEdit LineEdit { get; private set; }

	public IntProperty( Widget parent ) : base( parent )
	{
		LineEdit = new LineEdit( this );
		LineEdit.TextEdited += LineEdit_TextEdited;
		LineEdit.MinimumSize = Theme.RowHeight;
		LineEdit.NoSystemBackground = true;
		LineEdit.TranslucentBackground = true;
		Cursor = CursorShape.SizeH;

		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( 4096, Theme.RowHeight );
	}

	public IntProperty( string label, Widget parent ) : this( parent )
	{
		Label = label;
	}

	protected override void OnMouseEnter()
	{
		base.OnMouseEnter();
		Update();
	}

	protected override void OnMouseLeave()
	{
		base.OnMouseLeave();
		Update();
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		var h = Size.y;
		bool hovered = IsUnderMouse;
		if ( !Enabled ) hovered = false;

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;

		Paint.SetPenEmpty();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, Theme.ControlRadius );

		// icon box
		Paint.SetPenEmpty();
		Paint.SetBrush( HighlightColor.Darken( hovered ? 0.7f : 0.8f ).Desaturate( 0.8f ) );
		Paint.DrawRect( new Rect( 0, 0, h, h ).Grow( -1 ), Theme.ControlRadius - 1.0f );

		// flatten right (we need a DrawRect with uneven corners)
		Paint.DrawRect( new Rect( h - Theme.ControlRadius, 0, Theme.ControlRadius, h ).Grow( -1 ) );

		Paint.SetPen( HighlightColor.Darken( hovered ? 0.0f : 0.1f ).Desaturate( hovered ? 0.0f : 0.2f ) );

		if ( string.IsNullOrEmpty( Label ) )
		{
			Paint.DrawIcon( new Rect( 0, h ), Icon, h - 6, TextFlag.Center );
		}
		else
		{
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( new Rect( 1, h - 1 ), Label, TextFlag.Center );
		}
	}

	protected override void DoLayout()
	{
		base.DoLayout();
		var h = Size.y;
		LineEdit.Position = new Vector2( h, 0 );
		LineEdit.Size = Size - new Vector2( h, 0 );
	}

	private void LineEdit_TextEdited( string obj )
	{
		OnValueEdited?.Invoke();
	}

	float dragSpeed = 1.0f;
	Vector2 lastDragPosition;

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			lastDragPosition = e.LocalPosition;
			dragSpeed = 1;
		}
	}

	protected override void OnMouseMove( MouseEvent e )
	{
		base.OnMouseMove( e );

		if ( e.ButtonState.HasFlag( MouseButtons.Left ) )
		{
			var delta = e.LocalPosition - lastDragPosition;
			lastDragPosition = e.LocalPosition;

			dragSpeed = (e.LocalPosition.y + 100.0f) / 100.0f;
			dragSpeed = dragSpeed.Clamp( 0.001f, 1000.0f );

			Value += (int)(delta.x * 0.1f * dragSpeed);
			OnValueEdited?.Invoke();
		}


	}

}
