using Tools;
using SandMix.Nodes.Debug;

namespace SandMix.Tool.Inspector.NodeEditors;

[SandMixInspector( typeof( DebugTestNode ) )]
internal class TestNodeInspector : Widget
{
	public TestNodeInspector( Widget parent, DebugTestNode node ) : base( parent )
	{
		Layout = Layout.Grid();
		Layout.Add( new Label( "yaya", this ) );
		//Layout.Margin = new( 0, 0, 0, 0 );
		//SetSizeMode( SizeMode.CanShrink, SizeMode.CanShrink );
	}
}
