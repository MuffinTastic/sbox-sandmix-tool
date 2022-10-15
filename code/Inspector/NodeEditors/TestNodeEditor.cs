using System.Linq;
using Tools;
using SandMix.Nodes.Debug;
using Sandbox;
using Sandbox.UI;

namespace SandMix.Tool.Inspector.NodeEditors;

[SandMixInspector( typeof( DebugTestNode ) )]
internal class TestNodeEditor : BaseNodeEditor
{
	public TestNodeEditor( Widget parent, DebugTestNode node ) : base( parent )
	{
		var nodeProps = node.GetType().GetProperties();

		MinimumWidth = Width;

		Layout = Layout.Grid();
		Layout.Alignment = TextFlag.Center;

		Layout.Margin = new Margin( 10.0f, 10.0f, 10.0f, 10.0f );

		var prop = nodeProps.Where( t => t.Name == "RangedThingy" ).First();

		var slider = CreateSliderForProperty( node, prop, this );

		Layout.AddCell( 0, 1, slider );
	}
}
