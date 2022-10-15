using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

public class FloatSlider : PropertyEditorWidget, IEditorAttribute<MinMaxAttribute>, IEditorAttribute<Sandbox.RangeAttribute>
{
	public float Minimum { get; set; }
	public float Maximum { get; set; }
	public System.Action OnValueEdited { get; set; }

	float _value;

	float thumbHeight = 12;
	float thumbWidth = 5;

	public float Value
	{
		get => _value;
		set
		{
			var snapped = Step > 0 ? value.SnapToGrid( Step ) : value;
			snapped = snapped.Clamp( Minimum, Maximum);
			if ( _value == snapped ) return;

			_value = snapped;
			Update();
		}
	}

	public float DeltaValue
	{
		get
		{
			return MathX.LerpInverse( Value, Minimum, Maximum, true );
		}

		set
		{
			var v = MathX.LerpTo( Minimum, Maximum, value, true );
			Value = v;
		}

	}

	public float Step { get; set; } = 0.01f;

	public Color HighlightColor { get; set; } = Theme.Green;

	public void SetEditorAttribute( MinMaxAttribute attribute )
	{
		Minimum = attribute.MinValue;
		Maximum = attribute.MaxValue;
	}
		
	public void SetEditorAttribute( Sandbox.RangeAttribute attribute )
	{
		Minimum = attribute.Min;
		Maximum = attribute.Max;
	}

	public FloatSlider( Widget parent ) : base( parent )
	{
		Minimum = 0;
		Maximum = 100;
		Value = 25;

		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( 4096, Theme.RowHeight );
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

		var trackWidth = 5.0f;
		var center = Height / 2.0f;
		var halfThumb = thumbWidth * 0.5f;

		Paint.Antialiasing = true;

		var thumbPos = (DeltaValue * (Width - thumbWidth));

		// Track
		Paint.SetPenEmpty();

		Paint.SetBrush( HighlightColor.WithAlpha( 0.2f ) );
		Paint.DrawRect( new( halfThumb, center - trackWidth * 0.5f, Width - thumbWidth, trackWidth ), 3.0f );

		Paint.SetBrush( HighlightColor );
		Paint.DrawRect( new( halfThumb, center - trackWidth * 0.5f, thumbPos, trackWidth ), 3.0f );

		Paint.SetBrush( HighlightColor );
		Paint.SetPen( Color.Black.WithAlpha( 0.2f ), 1 );
		Paint.DrawRect( new( thumbPos, center - thumbHeight * 0.5f, thumbWidth, thumbHeight ), 2 );
	}


	void UpdateFromLocalPosition( float position )
	{
		var delta = (position - thumbWidth * 0.5f) / (Width - thumbWidth);
		DeltaValue = delta.Clamp( 0.0f, 1.0f );
		OnValueEdited?.Invoke();
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			UpdateFromLocalPosition( e.LocalPosition.x );
		}
	}

	protected override void OnMouseMove( MouseEvent e )
	{
		base.OnMouseMove( e );

		if ( e.ButtonState.HasFlag( MouseButtons.Left ) )
		{
			UpdateFromLocalPosition( e.LocalPosition.x );
		}
	}

}
