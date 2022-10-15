using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( typeof( RangedFloat ), "rangedfloat" )]
public class RangedFloatProperty : Widget
{
	FloatProperty X;
	FloatProperty Y;
	EnumProperty<RangedFloat.RangeType> EnumBox;

	public RangedFloat.RangeType RangeType { get; set; }

	public RangedFloat Value
	{
		get => new RangedFloat { x = X.Value, y = Y.Value, Range = RangeType };
		set
		{
			X.Value = value.x;
			Y.Value = value.y;
			RangeType = value.Range;

			RangeTypeChanged();
		}
	}

	public RangedFloatProperty( Widget parent ) : base( parent )
	{
		SetLayout( LayoutMode.LeftToRight );
		Layout.Spacing = 3;

		{
			X = Layout.Add( new FloatProperty( this ) { HighlightColor = Theme.Green, }, 1 );
		}

		{
			Y = Layout.Add( new FloatProperty( this ) { HighlightColor = Theme.Blue }, 1 );
		}


		{
			EnumBox = Layout.Add( new EnumProperty<RangedFloat.RangeType>( this ), -1 );
			EnumBox.Bind( "Value" ).From( this, x => x.RangeType );
			EnumBox.MinimumSize = new Vector2( 100, Theme.RowHeight );
			EnumBox.MaximumSize = EnumBox.MinimumSize;
		}
	}

	public void RangeTypeChanged()
	{
		if ( RangeType == RangedFloat.RangeType.Between )
		{
			X.Icon = "first_page";
			Y.Icon = "last_page";
			Y.Visible = true;
		}

		if ( RangeType == RangedFloat.RangeType.Fixed )
		{
			X.Icon = "tag";
			Y.Visible = false;
		}
	}

	public override void ChildValuesChanged( Widget source )
	{
		if ( source == EnumBox )
		{
			RangeTypeChanged();
		}

		base.ChildValuesChanged( source );
	}
}
