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
public class FloatingPointProperty<T> : Widget
{
	public string Label { get; set; }
	public string Icon { get; set; } = "multiple_stop";
	public Color HighlightColor = Theme.Green;

	public float DragValueSpeed { get; set; } = 1.0f;

	public event Action OnValueEdited;

	public T _value;

	public T Value
	{
		get => _value;

		set
		{
			if ( LineEdit.IsFocused ) return;
			_value = value;
			LineEdit_UpdatePrecision();
		}
	}

	public LineEdit LineEdit { get; private set; }

	public FloatingPointProperty( Widget parent ) : base( parent )
	{
		LineEdit = new LineEdit( this );
		LineEdit.TextEdited += LineEdit_TextEdited;
		LineEdit.EditingFinished += LineEdit_UpdatePrecision;
		LineEdit.EditingStarted += LineEdit_UpdatePrecision;
		LineEdit.MinimumSize = Theme.RowHeight;
		LineEdit.NoSystemBackground = true;
		LineEdit.TranslucentBackground = true;
		LineEdit.Alignment = TextFlag.LeftCenter;
		LineEdit.SetStyles( "background-color: transparent;" );
		Cursor = CursorShape.SizeH;

		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( 4096, Theme.RowHeight );
	}

	private void LineEdit_UpdatePrecision()
	{
		if ( LineEdit.IsFocused )
		{
			LineEdit.Text = ValueToText();
		}
		else
		{
			// This formatting causes real problems, such as "987654340" becoming "987654300" for no reason.
			LineEdit.Text = ValueToText( "0.##" );
		}
	}

	private void LineEdit_TextEdited( string obj )
	{
		TextToValue();
		OnValueEdited?.Invoke();
	}

	protected virtual void TextToValue() {}
	protected virtual string ValueToText( string format = null ) { return _value.ToString(); }

	public FloatingPointProperty( string label, Widget parent ) : this( parent )
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

	float dragSpeed = 1.0f;
	protected T dragValue;
	Vector2 lastDragPosition;

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			lastDragPosition = Application.CursorPosition;
			dragSpeed = 1;
			dragValue = Value;
			e.Accepted = true;
			return;
		}
	}

	protected virtual void OnDragValue( decimal add ) { /*dragValue += add;*/ }
	protected override void OnMouseMove( MouseEvent e )
	{
		base.OnMouseMove( e );

		if ( e.ButtonState.HasFlag( MouseButtons.Left ) )
		{
			var p = Application.CursorPosition;
			var delta = p - lastDragPosition;
			lastDragPosition = p;

			dragSpeed = (e.LocalPosition.y + 100.0f) / 100.0f;
			dragSpeed = dragSpeed.Clamp( 0.001f, 1000.0f );

			OnDragValue( (decimal)(delta.x * 0.1f * dragSpeed * DragValueSpeed) );
			Value = dragValue;

			OnValueEdited?.Invoke();
			SignalValuesChanged();

			e.Accepted = true;
			return;
		}
	}

}

// A bit of a hack because I hate copypasting code

[SandMixInspector( typeof( decimal ), "decimal" )]
public class DecimalProperty : FloatingPointProperty<decimal>
{
	public DecimalProperty( Widget parent ) : base( parent ) {}
	public DecimalProperty( string label, Widget parent ) : base( label, parent ) {}

	protected override void TextToValue() { _value = LineEdit.Text.ToDecimal(); }
	protected override string ValueToText( string format = null ) { return _value.ToString( format ); }
	protected override void OnDragValue( decimal add ) { dragValue += add; }
}

[SandMixInspector( typeof( double ), "double" )]
public class DoubleProperty : FloatingPointProperty<double>
{
	public DoubleProperty( Widget parent ) : base( parent ) {}
	public DoubleProperty( string label, Widget parent ) : base( label, parent ) {}

	protected override void TextToValue() { _value = (double)LineEdit.Text.ToDecimal(); }
	protected override string ValueToText( string format = null ) { return _value.ToString( format ); }
	protected override void OnDragValue( decimal add ) { dragValue += (double)add; }
}

[SandMixInspector( typeof( float ), "float" )]
public class FloatProperty : FloatingPointProperty<float>
{
	public FloatProperty( Widget parent ) : base( parent ) {}
	public FloatProperty( string label, Widget parent ) : base( label, parent ) {}

	protected override void TextToValue() { _value = LineEdit.Text.ToFloat(); }
	protected override string ValueToText( string format = null ) { return _value.ToString( format ); }
	protected override void OnDragValue( decimal add ) { dragValue += (float)add; }
}
