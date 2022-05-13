using SandMixTool.NodeGraph;
using SandMixTool.Nodes.Audio;
using SandMixTool.Nodes.Operations;
using System;
using Tools;

namespace SandMixTool.Widgets;

public class MixGraphWidget : DockWidget
{
	internal GraphView GraphView => Widget as GraphView;

	public MixGraphWidget( string title, Widget parent = null ) : base( title, "account_tree", parent )
	{

		CreateUI();
	}

	private void CreateUI()
	{
		Widget = new GraphView( this );

		GraphView.AddNodeType<TrackNode>( true );
		GraphView.AddNodeType<OutputNode>( true );

		GraphView.AddNodeType<FloatAddNode>( true );
		GraphView.AddNodeType<FloatSubNode>( true );
		GraphView.AddNodeType<Vec3AddNode>( true );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		var mainWindow = Parent as MainWindow;
		mainWindow.MixGraphFocus( this );
	}
}
