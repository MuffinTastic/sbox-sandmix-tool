using System;
using Tools;
using SandMix.Nodes;
using System.Reflection;
using Sandbox;

namespace SandMix.Tool.Inspector.NodeEditors;

public class BaseNodeEditor : Widget
{
	protected class NodePropertyAttribute : Attribute
	{

	}

	protected BaseNodeEditor( Widget parent ) : base( parent )
	{

	}

	protected static Widget CreateSliderForProperty( BaseNode node, PropertyInfo prop, Widget parent )
	{
		var di = DisplayInfo.ForMember( prop );

		var sliderGroup = new Widget( parent );
		sliderGroup.Layout = Layout.Column();
		sliderGroup.Layout.Alignment = TextFlag.Right;

		var label = new Label( di.Name );
		label.Alignment = TextFlag.CenterHorizontally;
		label.ToolTip = di.Description;
		sliderGroup.Layout.Add( label, 0 );

		var spacerGroup = sliderGroup.Layout.Add( LayoutMode.LeftToRight, 1 );

		var floatSlider = new Widgets.VerticalFloatSlider( sliderGroup );
		floatSlider.Bind( "Value" ).From( node, prop );
		floatSlider.ToolTip = di.Description;

		spacerGroup.AddStretchCell( 1 );
		spacerGroup.Add( floatSlider, 0 );
		spacerGroup.AddStretchCell( 1 );

		return sliderGroup;
	}
}
