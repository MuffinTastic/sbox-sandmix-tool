﻿using Sandbox;
using Sandbox.Internal;
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
[Tool( SandMixTool.ProjectName, "surround_sound", SandMixTool.ProjectTagline )]
public class MainWindow : Window, IAssetEditor
{
	public static MainWindow Instance { get; private set; }

	public bool CanOpenMultipleAssets => throw new NotImplementedException();

	private InspectorWidget Inspector;
	private PreviewWidget Preview;

	private MixGraphWidget CurrentMixGraph;
	private List<MixGraphWidget> MixGraphs = new();

	private DockWidget NewFileHandle;

	private Option GraphSaveOption;
	private Option GraphSaveAsOption;

	private Option GraphUndoOption;
	private Option GraphRedoOption;
	private Option GraphCutOption;
	private Option GraphCopyOption;
	private Option GraphPasteOption;
	private Option GraphDeleteOption;

	public MainWindow()
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

		CreateUI();
		Show();

		// created through toolbar button
	}

	public MainWindow( Widget parent = null ) : base( parent )
	{
		Close();

		// created through inspector
	}

	public void AssetOpen( Asset asset )
	{
		Instance ??= new MainWindow();
		Instance.OpenMixGraph( asset.AbsolutePath );
		Instance.Focus();
	}

	public void BuildMenu()
	{
		MenuBar.Clear();

		var file = MenuBar.AddMenu( "File" );
		{
			var newMix = file.AddOption( "New" );
			newMix.Triggered += () => CreateMixGraph();
			newMix.Shortcut = "Ctrl+N";

			var openMix = file.AddOption( "Open" );
			openMix.Triggered += () => OpenMixGraphFromChooser();
			openMix.Shortcut = "Ctrl+O";

			file.AddSeparator();

			GraphSaveOption = new Option( title: "Save", icon: null, action: () => CurrentMixGraph?.Save() );
			GraphSaveOption.Shortcut = "Ctrl+S";
			file.AddOption( GraphSaveOption );

			GraphSaveAsOption = new Option( title: "Save As", icon: null, action: () => CurrentMixGraph?.SaveAs() );
			file.AddOption( GraphSaveAsOption );

			file.AddSeparator();

			file.AddOption( "Quit" ).Triggered += () => Close();
		}

		var edit = MenuBar.AddMenu( "Edit" );
		{
			GraphUndoOption = new Option( title: "Undo", icon: null, action: () => CurrentMixGraph?.GraphUndo() );
			GraphUndoOption.Shortcut = "Ctrl+Z";
			edit.AddOption( GraphUndoOption );

			GraphRedoOption = new Option( title: "Redo", icon: null, action: () => CurrentMixGraph?.GraphRedo() );
			GraphRedoOption.Shortcut = "Ctrl+Y";
			edit.AddOption( GraphRedoOption );

			edit.AddSeparator();

			GraphCutOption = new Option( title: "Cut", icon: null, action: () => CurrentMixGraph?.GraphCut() );
			GraphCutOption.Shortcut = "Ctrl+X";
			edit.AddOption( GraphCutOption );

			GraphCopyOption = new Option( title: "Copy", icon: null, action: () => CurrentMixGraph?.GraphCopy() );
			GraphCopyOption.Shortcut = "Ctrl+C";
			edit.AddOption( GraphCopyOption );

			GraphPasteOption = new Option( title: "Paste", icon: null, action: () => CurrentMixGraph?.GraphPaste() );
			GraphPasteOption.Shortcut = "Ctrl+V";
			edit.AddOption( GraphPasteOption );

			GraphDeleteOption = new Option( title: "Delete", icon: null, action: () => CurrentMixGraph?.GraphDelete() );
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

		var mixGraphFocused = CurrentMixGraph is not null;
		GraphSaveOption.Enabled = mixGraphFocused;
		GraphSaveAsOption.Enabled = mixGraphFocused;

		GraphUndoOption.Enabled = CurrentMixGraph?.GraphCanUndo() ?? false;
		GraphRedoOption.Enabled = CurrentMixGraph?.GraphCanRedo() ?? false;

		var hasSelection = CurrentMixGraph?.GraphHasSelection() ?? false;
		GraphCutOption.Enabled = hasSelection;
		GraphCopyOption.Enabled = hasSelection;
		GraphPasteOption.Enabled = mixGraphFocused;
		GraphDeleteOption.Enabled = hasSelection;
	}

	public void CreateUI()
	{
		//Clear();
		CurrentMixGraph = null;

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

		CreateMixGraph();
	}

	public MixGraphWidget CreateMixGraph()
	{
		var mixGraph = new MixGraphWidget( this );
		mixGraph.MixGraphFocus += OnMixGraphFocus;
		mixGraph.MixGraphClose += OnMixGraphClose;
		mixGraph.GraphView.GraphUpdated += UpdateMenuBar;

		if ( Inspector is not null )
			mixGraph.GraphView.NodeSelect += Inspector.StartInspecting;
		else
			Log.Warning( "Inspector was null" );

		//var lastOpened = Children.OfType<MixGraphWidget>().Where( mg => mg != mixGraph ).LastOrDefault();

		//if ( lastOpened is not null )
			DockInTab( NewFileHandle, mixGraph );
		//else
		//	Dock( mixGraph, DockArea.Left, null );

		mixGraph.Show();
		mixGraph.Raise();

		CurrentMixGraph = mixGraph;
		MixGraphs.Add( mixGraph );

		UpdateMenuBar();

		return mixGraph;
	}

	public void OpenMixGraph( string path )
	{
		var openMixGraph = MixGraphs.Where( mg => mg.Asset?.AbsolutePath.ToLower() == path.ToLower() ).FirstOrDefault();

		if ( openMixGraph is not null )
		{
			openMixGraph.Raise();
			return;
		}

		var mixGraph = CurrentMixGraph;

		Log.Info( $"DEBUG C {mixGraph}" ); 
		Log.Info( $"DEBUG C C {mixGraph?.Changed}" );
		Log.Info( $"DEBUG C A {mixGraph?.Asset}" );

		if ( mixGraph is null || mixGraph.Changed || mixGraph.Asset is not null )
			mixGraph = CreateMixGraph();

		mixGraph.ReadFromDisk( path );

		CurrentMixGraph = mixGraph;
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
			OpenMixGraph( fd.SelectedFile );
		}
	}

	protected override void OnClosed()
	{
		if ( Instance == this )
		{
			Instance = null;
		}

		var unsavedMixGraphs = MixGraphs.Where( mg => mg.Changed && mg.AttemptSave );

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

	public void OnMixGraphFocus( MixGraphWidget mixGraph )
	{
		CurrentMixGraph = mixGraph;

		var otherMixGraphs = MixGraphs.Where( mg => mg != mixGraph );
		foreach ( var otherMixGraph in otherMixGraphs )
		{
			otherMixGraph.GraphView.UnfocusAllNodes();
		}

		UpdateMenuBar();
	}

	public void OnMixGraphClose( MixGraphWidget mixGraph )
	{
		if ( mixGraph == CurrentMixGraph )
			CurrentMixGraph = null;

		MixGraphs.Remove( mixGraph );

		UpdateMenuBar();
	}
}
