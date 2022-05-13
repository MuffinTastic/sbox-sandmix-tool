using Sandbox;
using SandMixTool.NodeGraph;
using SandMixTool.Widgets;
using System;
using System.Text.Json;
using Tools;

namespace SandMixTool;

[Tool( SandMixTool.ProjectName, "surround_sound", SandMixTool.ProjectTagline )]
public class MainWindow : Window
{
	public Action<MixGraphWidget> MixGraphFocus;

	private PreviewWidget Preview;
	private InspectorWidget Inspector;
	private MixGraphWidget CurrentMixGraph;

	public MainWindow()
	{
		Title = SandMixTool.ProjectName;
		Size = new Vector2( 1920, 1080 );

		CreateUI();
		Show();
	}

	public void BuildMenu()
	{
		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "Open" );
		file.AddOption( "Save" ).Triggered += () => SaveMixGraph();
		file.AddOption( "Quit" ).Triggered += () => Close();

		var view = MenuBar.AddMenu( "View" );

		var help = MenuBar.AddMenu( "Help" );
		help.AddOption( "Documentation" );
		help.AddSeparator();
		help.AddOption( "About" ).Triggered += () => new Dialogs.AboutDialog( this ).Show();
	}

	public void CreateUI()
	{
		Clear();

		BuildMenu();

		MixGraphFocus += SetGraphView;

		Preview = new PreviewWidget( null, this );
		Dock( Preview, DockArea.Right );

		Inspector = new InspectorWidget( this );
		Dock( Inspector, DockArea.Right );

		CreateMixGraph();
		CreateMixGraph();
	}

	public void CreateMixGraph()
	{
		var mg = new MixGraphWidget( "Mix Graph", this );
		Dock( mg, DockArea.Left, CurrentMixGraph );

		mg.GraphView.NodeSelect += Inspector.StartInspecting;
	}

	public void SetGraphView( MixGraphWidget graphView )
	{
		CurrentMixGraph?.GraphView.UnfocusAllNodes();
		CurrentMixGraph = graphView;
	}

	public void SaveMixGraph()
	{
		if ( CurrentMixGraph is null )
			return;

		var fd = new FileDialog( this );

		fd.Title = "Save as...";
		fd.SetNameFilter( SandMixTool.FileFilter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			CurrentMixGraph.GraphView.SaveGraph( fd.SelectedFile );
		}
	}

	[Sandbox.Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}
}
