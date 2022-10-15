using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.NodeEditors.Widgets;

public class VerticalFloatSlider : PropertyEditorWidget, IEditorAttribute<MinMaxAttribute>, IEditorAttribute<Sandbox.RangeAttribute>
{
	public float Minimum { get; set; }
	public float Maximum { get; set; }
	public System.Action OnValueEdited { get; set; }

	float _value;

	float thumbHeight = 5;
	float thumbWidth = 12;

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

	public VerticalFloatSlider( Widget parent ) : base( parent )
	{
		Minimum = 0;
		Maximum = 100;
		Value = 25;

		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( Theme.RowHeight, 4096  );
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
		var center = Width / 2.0f;
		var halfThumb = thumbHeight * 0.5f;

		Paint.Antialiasing = true;

		var thumbPos = Height - (DeltaValue * (Height - thumbHeight));

		// Track
		Paint.SetPenEmpty();

		Paint.SetBrush( HighlightColor.WithAlpha( 0.2f ) );
		Paint.DrawRect( new( center - trackWidth * 0.5f, halfThumb, trackWidth, Height - thumbHeight ), 3.0f );

		Paint.SetBrush( HighlightColor );
		Paint.DrawRect( new( center - trackWidth * 0.5f, halfThumb + thumbPos, trackWidth, Height - thumbPos - thumbHeight ), 3.0f );

		Paint.SetBrush( HighlightColor );
		Paint.SetPen( Color.Black.WithAlpha( 0.2f ), 1 );
		Paint.DrawRect( new( center - thumbWidth * 0.5f, thumbPos - halfThumb, thumbWidth, thumbHeight ), 2 );
	}


	void UpdateFromLocalPosition( float position )
	{
		var delta = 1.0f - (position - thumbHeight * 0.5f) / (Height - thumbHeight);
		DeltaValue = delta.Clamp( 0.0f, 1.0f );
		OnValueEdited?.Invoke();
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			UpdateFromLocalPosition( e.LocalPosition.y );
		}
	}

	protected override void OnMouseMove( MouseEvent e )
	{
		base.OnMouseMove( e );

		if ( e.ButtonState.HasFlag( MouseButtons.Left ) )
		{
			UpdateFromLocalPosition( e.LocalPosition.y );
		}
	}

}
