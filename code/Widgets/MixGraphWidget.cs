using SandMixTool.NodeGraph;
using SandMixTool.Nodes.Audio;
using SandMixTool.Nodes.Operations;
using System;
using Tools;

namespace SandMixTool.Widgets;

public class MixGraphWidget : DockWidget
{
	internal GraphView GraphView => Widget as GraphView;

	public MixGraphWidget( Widget parent = null ) : base( "Mix Graph", "account_tree", parent )
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

	public void SaveGraph()
	{
		// open file dialog to get path
		GraphView.SaveGraph( " " );
	}
}
