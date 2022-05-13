using Sandbox;
using SandMixTool.NodeGraph;
using SandMixTool.Widgets;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Tools;

namespace SandMixTool;

[Tool( SandMixTool.ProjectName, "surround_sound", SandMixTool.ProjectTagline )]
public class MainWindow : Window
{
	public Action<MixGraphWidget> MixGraphFocus;
	public Action<MixGraphWidget> MixGraphClose;

	private PreviewWidget Preview;
	private InspectorWidget Inspector;
	private MixGraphWidget CurrentMixGraph;
	private List<MixGraphWidget> MixGraphs = new();

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
		file.AddOption( "Save As" ).Triggered += () => SaveMixGraphAs();
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

		MixGraphFocus += OnMixGraphFocus;
		MixGraphClose += OnMixGraphClose;

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
		MixGraphs.Add( mg );
	}

	public void OnMixGraphFocus( MixGraphWidget mixGraph )
	{
		CurrentMixGraph?.GraphView.UnfocusAllNodes();
		CurrentMixGraph = mixGraph;
	}

	public void OnMixGraphClose( MixGraphWidget mixGraph )
	{
		MixGraphs.Remove( mixGraph );
	}

	public async void SaveMixGraph( )
	{

	}

	public async void SaveMixGraphAs( MixGraphWidget mixGraph )
	{
		if ( mixGraph is null )
			return;

		var fd = new FileDialog( CurrentMixGraph );

		fd.Title = "Save as...";
		fd.SetNameFilter( SandMixTool.FileFilter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			await mixGraph.WriteToDisk
		}
	}

	[Sandbox.Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}
}
