using System;
using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

/// <summary>
/// A draggable slider for floats with a text entry for manual input on the right.
/// </summary>
[SandMixInspector( typeof( float ), "float" )]
public class FloatSliderProperty : PropertyEditorWidget, IEditorAttribute<MinMaxAttribute>, IEditorAttribute<Sandbox.RangeAttribute>
{
	const float NumberFieldWidth = 80;

	public string Label { get; set; }
	public string Icon { get; set; } = "multiple_stop";
	public Color HighlightColor { get; set; } = Theme.Green;

	public float Minimum
	{
		get => FloatSlider.Minimum;

		set
		{
			if ( value == FloatSlider.Minimum ) return;

			FloatSlider.Minimum = value;

			if ( FloatSlider.Value < FloatSlider.Minimum ) { }
				Value = FloatSlider.Minimum;
		}
	}

	public float Maximum
	{
		get => FloatSlider.Maximum;

		set
		{
			if ( value == FloatSlider.Maximum ) return;

			FloatSlider.Maximum = value;

			if ( FloatSlider.Value > FloatSlider.Maximum )
				Value = FloatSlider.Maximum;
		}
	}

	public float Step
	{
		get => FloatSlider.Step;
		set => FloatSlider.Step = value;
	}

	public string Format { get; set; } = "0.##";

	public event Action OnValueEdited;

	public float Value
	{
		get => FloatSlider.Value;

		set
		{
			FloatSlider.Value = value;
			LineEdit.Text = FloatSlider.Value.ToString( Format );
		}
	}

	public LineEdit LineEdit { get; private set; }
	public FloatSlider FloatSlider { get; private set; }

	public void SetEditorAttribute( MinMaxAttribute attribute )
	{
		FloatSlider.SetEditorAttribute( attribute );
	}

	public void SetEditorAttribute( Sandbox.RangeAttribute attribute )
	{
		FloatSlider.SetEditorAttribute( attribute );
	}

	public FloatSliderProperty( Widget parent ) : base( parent )
	{
		LineEdit = new LineEdit( this );
		LineEdit.TextEdited += LineEdit_TextEdited;
		LineEdit.EditingFinished += LineEdit_EditingFinished;
		LineEdit.MinimumSize = Theme.RowHeight;
		LineEdit.NoSystemBackground = true;
		LineEdit.TranslucentBackground = true;
		LineEdit.Alignment = TextFlag.RightCenter;

		FloatSlider = new FloatSlider( this );
		FloatSlider.OnValueEdited = () =>
		{
			Value = FloatSlider.Value;
			SignalValuesChanged();
			OnValueEdited?.Invoke();
		};

		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( 4096, Theme.RowHeight );
	}

	public FloatSliderProperty( string label, Widget parent ) : this( parent )
	{
		Label = label;
	}

	protected override void DoLayout()
	{
		base.DoLayout();
		var h = Size.y;


		FloatSlider.Position = new Vector2( 5, 0 );
		FloatSlider.Size = new Vector2( Size.x - NumberFieldWidth - 10, h );

		LineEdit.Position = new Vector2( Size.x - NumberFieldWidth, 0 );
		LineEdit.Size = new Vector2( NumberFieldWidth, h );
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.Antialiasing = true;

		Paint.SetPenEmpty();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, Theme.ControlRadius );
	}

	private void LineEdit_EditingFinished()
	{
		FloatSlider.Value = LineEdit.Text.ToFloat();
		LineEdit.Text = FloatSlider.Value.ToString( Format );
		OnValueEdited?.Invoke();
	}

	private void LineEdit_TextEdited( string obj )
	{
		FloatSlider.Value = LineEdit.Text.ToFloat();
		OnValueEdited?.Invoke();
	}
}
