using Sandbox;
using Sandbox.Internal;
using SandMix;
using SandMix.Nodes;
using SandMixTool.Dialogs;
using SandMixTool.NodeGraph;
using SandMixTool.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Tools;
using Tools.NodeEditor;

namespace SandMixTool;

[CanEdit( "asset:smix" )]
[CanEdit( "asset:smixefct" )]
public class DummyWindow : Window, IAssetEditor
{
	public bool CanOpenMultipleAssets => throw new NotImplementedException();

	public DummyWindow( Widget parent = null ) : base( parent )
	{
		Close();

		// created through inspector
	}

	public void AssetOpen( Asset asset )
	{
		MainWindow.Instance ??= new MainWindow( createMix: false );
		MainWindow.Instance.OpenNodeGraph( asset.AbsolutePath );
		MainWindow.Instance.Focus();
	}
}

[Tool( SandMixTool.ProjectName, "surround_sound", SandMixTool.ProjectTagline )]
public class MainWindow : Window
{
	public static MainWindow Instance { get; set; }


	private InspectorWidget Inspector;
	private PreviewWidget Preview;

	private NodeGraphWidget CurrentNodeGraph;
	private List<NodeGraphWidget> NodeGraphs = new();

	private DockWidget NewFileHandle;

	private Option GraphSaveOption;
	private Option GraphSaveAsOption;

	private Option GraphUndoOption;
	private Option GraphRedoOption;
	private Option GraphCutOption;
	private Option GraphCopyOption;
	private Option GraphPasteOption;
	private Option GraphDeleteOption;

	public MainWindow() : this( createMix: true )
	{

	}

	public MainWindow( bool createMix )
	{
		if ( Instance is not null )
		{
			Hide();
			Close();

			Instance.Focus();
			return;
		}

		Instance = this;

		Title = SandMixTool.ProjectName;
		Size = new Vector2( 1920, 1080 );

		CreateUI( createMix );
		Show();

		// created through toolbar button
	}

	public void BuildMenu()
	{
		MenuBar.Clear();

		var file = MenuBar.AddMenu( "File" );
		{
			var newMix = file.AddOption( "New Mix" );
			newMix.Triggered += () => CreateNodeGraph( MixGraphResource.FileExtension );
			newMix.Shortcut = "Ctrl+N";

			var newEffect = file.AddOption( "New Effect" );
			newEffect.Triggered += () => CreateNodeGraph( EffectResource.FileExtension );
			newEffect.Shortcut = "Ctrl+Shift+N";

			var open = file.AddOption( "Open" );
			open.Triggered += () => OpenMixGraphFromChooser();
			open.Shortcut = "Ctrl+O";

			file.AddSeparator();

			GraphSaveOption = new Option( title: "Save", icon: null, action: () => CurrentNodeGraph?.Save() );
			GraphSaveOption.Shortcut = "Ctrl+S";
			file.AddOption( GraphSaveOption );

			GraphSaveAsOption = new Option( title: "Save As", icon: null, action: () => CurrentNodeGraph?.SaveAs() );
			file.AddOption( GraphSaveAsOption );

			file.AddSeparator();

			file.AddOption( "Quit" ).Triggered += () => Close();
		}

		var edit = MenuBar.AddMenu( "Edit" );
		{
			GraphUndoOption = new Option( title: "Undo", icon: null, action: () => CurrentNodeGraph?.GraphUndo() );
			GraphUndoOption.Shortcut = "Ctrl+Z";
			edit.AddOption( GraphUndoOption );

			GraphRedoOption = new Option( title: "Redo", icon: null, action: () => CurrentNodeGraph?.GraphRedo() );
			GraphRedoOption.Shortcut = "Ctrl+Y";
			edit.AddOption( GraphRedoOption );

			edit.AddSeparator();

			GraphCutOption = new Option( title: "Cut", icon: null, action: () => CurrentNodeGraph?.GraphCut() );
			GraphCutOption.Shortcut = "Ctrl+X";
			edit.AddOption( GraphCutOption );

			GraphCopyOption = new Option( title: "Copy", icon: null, action: () => CurrentNodeGraph?.GraphCopy() );
			GraphCopyOption.Shortcut = "Ctrl+C";
			edit.AddOption( GraphCopyOption );

			GraphPasteOption = new Option( title: "Paste", icon: null, action: () => CurrentNodeGraph?.GraphPaste() );
			GraphPasteOption.Shortcut = "Ctrl+V";
			edit.AddOption( GraphPasteOption );

			GraphDeleteOption = new Option( title: "Delete", icon: null, action: () => CurrentNodeGraph?.GraphDelete() );
			GraphDeleteOption.Shortcut = "Del";
			edit.AddOption( GraphDeleteOption );
		}

		var view = MenuBar.AddMenu( "View" );
		{
			view.AddOption( Preview.GetToggleViewOption() );
			view.AddOption( Inspector.GetToggleViewOption() );
		}

		var help = MenuBar.AddMenu( "Help" );
		{
			help.AddOption( "Documentation" );
			help.AddSeparator();
			help.AddOption( "About" ).Triggered += () => new Dialogs.AboutDialog( this ).Show();
		}
	}

	public void UpdateMenuBar()
	{
		if ( !IsValid ) return;

		var nodeGraphFocused = CurrentNodeGraph is not null;
		GraphSaveOption.Enabled = nodeGraphFocused;
		GraphSaveAsOption.Enabled = nodeGraphFocused;

		GraphUndoOption.Enabled = CurrentNodeGraph?.GraphCanUndo() ?? false;
		GraphRedoOption.Enabled = CurrentNodeGraph?.GraphCanRedo() ?? false;

		var hasSelection = CurrentNodeGraph?.GraphHasSelection() ?? false;
		GraphCutOption.Enabled = hasSelection;
		GraphCopyOption.Enabled = hasSelection;
		GraphPasteOption.Enabled = nodeGraphFocused;
		GraphDeleteOption.Enabled = hasSelection;
	}

	public void CreateUI( bool createMix )
	{
		//Clear();
		CurrentNodeGraph = null;

		NewFileHandle = new DockWidget( "", null, this );
		Dock( NewFileHandle, DockArea.Left );
		NewFileHandle.Hide();

		Preview = new PreviewWidget( null, this );
		Preview.MinimumSize = (Vector2) 300.0f;
		Dock( Preview, DockArea.Right );
		Preview.Show();

		Inspector = new InspectorWidget( this );
		Inspector.MinimumSize = (Vector2) 300.0f;
		Dock( Inspector, DockArea.Right );
		Inspector.Show();

		BuildMenu();

		if ( createMix )
		{
			CreateNodeGraph( MixGraphResource.FileExtension );
		}
	}

	public NodeGraphWidget CreateNodeGraph( string extension )
	{

		NodeGraphWidget nodeGraph;

		nodeGraph = extension switch
		{
			MixGraphResource.FileExtension => new MixGraphWidget( this ),
			EffectResource.FileExtension => new EffectGraphWidget( this ),
			_ => throw new NotImplementedException( extension )
		};

		nodeGraph.NodeGraphFocus += OnMixGraphFocus;
		nodeGraph.NodeGraphClose += OnMixGraphClose;
		nodeGraph.GraphView.GraphUpdated += UpdateMenuBar;

		if ( Inspector is not null )
			nodeGraph.GraphView.NodeSelect += Inspector.StartInspecting;
		else
			Log.Warning( "Inspector was null" );

		DockInTab( NewFileHandle, nodeGraph );

		nodeGraph.Show();
		nodeGraph.Raise();

		CurrentNodeGraph = nodeGraph;
		NodeGraphs.Add( nodeGraph );

		UpdateMenuBar();

		return nodeGraph;
	}

	public void OpenNodeGraph( string path )
	{
		var openMixGraph = NodeGraphs.Where( mg => mg.Asset?.AbsolutePath.ToLower() == path.ToLower() ).FirstOrDefault();

		if ( openMixGraph is not null )
		{
			openMixGraph.Raise();
			return;
		}

		var nodeGraph = CurrentNodeGraph;

		Log.Info( $"DEBUG C {nodeGraph}" ); 
		Log.Info( $"DEBUG C C {nodeGraph?.Changed}" );
		Log.Info( $"DEBUG C A {nodeGraph?.Asset}" );

		var asset = AssetSystem.FindByPath( path );

		if ( nodeGraph is null || nodeGraph.Changed || nodeGraph.Asset?.AssetType != asset.AssetType )
			nodeGraph = CreateNodeGraph( asset.AssetType.FileExtension );

		nodeGraph.ReadAsset( asset );

		CurrentNodeGraph = nodeGraph;
	}

	public void OpenMixGraphFromChooser()
	{
		Raise();
		var fd = new FileDialog( this );

		fd.Title = $"Open";
		fd.SetNameFilter( SandMixTool.FindFileFilter );
		fd.SetFindExistingFile();

		if ( fd.Execute() )
		{
			OpenNodeGraph( fd.SelectedFile );
		}
	}

	protected override void OnClosed()
	{
		if ( Instance == this )
		{
			Instance = null;
		}

		var unsavedMixGraphs = NodeGraphs.Where( mg => mg.Changed && mg.AttemptSave );

		if ( unsavedMixGraphs.Count() == 0 )
		{
			return;
		}

		var closeConfirm = new SaveDialog( unsavedMixGraphs, this );
		closeConfirm.Triggered += ( result ) =>
		{
			if ( result == SaveDialog.Result.Cancel )
				return;

			foreach ( var mixGraph in unsavedMixGraphs )
			{
				switch ( result )
				{
					case SaveDialog.Result.Yes:
						mixGraph.Save();
						break;
					case SaveDialog.Result.No:
						mixGraph.DontAttemptSave();
						break;
				}
			}

			Close();
		};
	}

	public void OnMixGraphFocus( NodeGraphWidget mixGraph )
	{
		CurrentNodeGraph = mixGraph;

		var otherMixGraphs = NodeGraphs.Where( mg => mg != mixGraph );
		foreach ( var otherMixGraph in otherMixGraphs )
		{
			otherMixGraph.GraphView.UnfocusAllNodes();
		}

		UpdateMenuBar();
	}

	public void OnMixGraphClose( NodeGraphWidget mixGraph )
	{
		if ( mixGraph == CurrentNodeGraph )
			CurrentNodeGraph = null;

		NodeGraphs.Remove( mixGraph );

		UpdateMenuBar();
	}
}
