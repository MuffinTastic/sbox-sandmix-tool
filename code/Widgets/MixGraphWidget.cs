using SandMixTool.NodeGraph;
using System;
using System.IO;
using Tools;

namespace SandMixTool.Widgets;

public class MixGraphWidget : DockWidget
{
	private MainWindow MainWindow => Parent as MainWindow;

	internal GraphView GraphView => Widget as GraphView;
	public string Path { get; set; }
	

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

	public async void WriteToDisk()
	{
		if ( string.IsNullOrEmpty( Path ) )
			return;

		var data = GraphView.Graph.Serialize();
		
		await File.WriteAllTextAsync( Path, data );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		MainWindow.MixGraphFocus( this );
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		MainWindow.MixGraphClose( this );
	}
}
