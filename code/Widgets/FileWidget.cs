﻿using System;
using System.IO;
using System.Linq;
using Sandbox;
using Tools;
using SandMix.Nodes;
using SandMix.Tool.NodeGraph;
using SandMix.Nodes.Mix;
using SandMix.Nodes.Effects;
using SandMix.Tool.Dialogs;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections.Generic;

namespace SandMix.Tool.Widgets;

public class FileWidget : DockWidget
{
	private static Dictionary<string, FileWidget> OpenFiles = new();

	private ToolWindow.MenuBarOptions MenuOptions;
	private PreviewWidget Preview;
	private InspectorWidget Inspector;

	private Asset Asset = null;
	private GraphContainer Graph = null;
	private GraphView GraphView
	{
		get => Widget as GraphView;
		set => Widget = value;
	}

	private bool UnsavedChanges = false;

	public FileWidget( Widget parent, PreviewWidget preview, InspectorWidget inspector )
		: base( null, SandMixTool.BaseIcon, parent )
	{
		SetTitle();
		Preview = preview;
		Inspector = inspector;

		UpdateWindowDressing();
	}
	
	private void SetTitle()
	{
		Title = Util.GetLocalized( "#smix.ui.graphview" );
	}

	public void UpdateLanguage()
	{
		if ( !IsValid )
		{
			return;
		}

		SetTitle();
		GraphView?.UpdateLanguage();

		Update();
	}

	public void SetMenuBarOptions( ToolWindow.MenuBarOptions menuOptions )
	{
		MenuOptions = menuOptions;
		UpdateMenuBar();
	}

	private void UpdateWindowDressing()
	{
		var window = Parent as ToolWindow;

		if ( !window.IsValid )
		{
			return;
		}

		if ( !FileIsOpen )
		{
			window.ResetWindowIcon();
			window.ResetWindowTitle();
			return;
		}

		var icon = Graph.GraphType switch
		{
			GraphType.Mix => MixGraphResource.Icon,
			GraphType.Effect => EffectGraphResource.Icon,
			_ => throw new Exception( "Unknown graph type" )
		};

		string title;

		if ( Asset is not null )
		{
			title = Path.GetFileName( Asset.AbsolutePath );
		}
		else
		{
			title = Graph.GraphType switch
			{
				GraphType.Mix => Util.GetLocalized( "#smix.ui.title.newmixgraph" ),
				GraphType.Effect => Util.GetLocalized( "#smix.ui.title.neweffectgraph" ),
				_ => throw new Exception( "Unknown graph type" )
			};
		}

		if ( UnsavedChanges )
		{
			title += "*";
		}

		window.SetWindowIcon( icon );
		window.SetWindowTitle( title );
	}

	private void UpdateMenuBar()
	{
		if ( !Parent.IsValid )
		{
			return;
		}

		var fileOpen = FileIsOpen;
		MenuOptions.FileCloseOption.Enabled = fileOpen;
		MenuOptions.FileSaveOption.Enabled = fileOpen;
		MenuOptions.FileSaveAsOption.Enabled = fileOpen;

		MenuOptions.EditUndoOption.Enabled = GraphView?.CanUndo() ?? false;
		MenuOptions.EditRedoOption.Enabled = GraphView?.CanRedo() ?? false;

		var hasSelection = GraphView?.HasSelection() ?? false;
		MenuOptions.EditCutOption.Enabled = hasSelection;
		MenuOptions.EditCopyOption.Enabled = hasSelection;
		MenuOptions.EditPasteOption.Enabled = fileOpen;
		MenuOptions.EditDeleteOption.Enabled = hasSelection;

		MenuOptions.ViewRecenterGraphViewOption.Enabled = fileOpen;
	}

	private void SetupGraph( GraphContainer graph )
	{
		Graph = graph;
		GraphView = new GraphView( Graph, this );
		UpdateWindowDressing();
		UpdateMenuBar();

		GraphView.GraphUpdated += () => {
			UnsavedChanges = true;
			UpdateWindowDressing();
			UpdateMenuBar();
		};

		GraphView.NodeSelect += ( _, _ ) => UpdateMenuBar();
		GraphView.OnSelectionChanged += () => UpdateMenuBar();

		GraphView.NodeSelect += Inspector.StartInspecting;
	}

	private void Reset()
	{
		GraphView?.Destroy();
		GraphView = null;
		Graph = null;
		Asset = null;

		Inspector.StartInspecting( null );
	}

	private void WriteAsset()
	{
		(var resource, _) = SandMixResource.GetFromAsset( Asset );
		Assert.NotNull( resource );

		resource.JsonData = GraphView.Graph.Serialize();
		Asset.SaveToDisk( resource );
		Asset.Compile( full: true );

		UnsavedChanges = false;
		UpdateWindowDressing();
	}

	public bool FileIsOpen => Graph is not null;

	public async Task FileNew( GraphType graphType )
	{
		if ( !await FileClose() )
		{
			return;
		}

		SetupGraph( new GraphContainer( graphType ) );
	}

	public async Task FileOpen( Asset asset = null )
	{
		if ( !await FileClose() )
		{
			return;
		}

		if ( asset is null )
		{
			var fd = new FileDialog( this );

			fd.Title = $"Open";
			fd.SetNameFilter( Util.GetFindFileFilter() );
			fd.SetFindExistingFile();

			if ( !fd.Execute() ) // canceled by user
			{
				return;
			}

			var file = fd.SelectedFile;
			asset = AssetSystem.FindByPath( file );

			if ( asset is null ) // file isn't there?
			{
				Log.Error( new FileNotFoundException( file ) );
			}
		}

		if ( OpenFiles.TryGetValue( asset.AbsolutePath, out FileWidget openFile ) )
		{
			openFile.Focus();
			(Parent as ToolWindow).Close();
			return;
		}

		Asset = asset;

		(var resource, var graphType) = SandMixResource.GetFromAsset( Asset );

		if ( resource?.JsonData is not null )
		{
			Graph = GraphContainer.Deserialize( resource.JsonData );
		}
		else
		{
			Log.Warning( $"Json data was null while reading from {asset.RelativePath}" );
			Graph = new GraphContainer( graphType );
		}

		SetupGraph( Graph );

		OpenFiles.Add( Asset.AbsolutePath, this );
	}

	public async Task<bool> FileClose()
	{
		// FIXME: CloseEvent not being there causes weird shit, patch it here for now
		if ( Asset is not null && OpenFiles.ContainsKey( Asset.AbsolutePath ) )
		{
			OpenFiles.Remove( Asset.AbsolutePath );
		}

		if ( FileIsOpen && UnsavedChanges )
		{
			var result = await SaveDialog.RunAsync( Parent );

			switch ( result )
			{
				case SaveDialog.Result.Yes:
					Save();
					break;

				case SaveDialog.Result.No:
					// continue on to destroy
					break;

				case SaveDialog.Result.Cancel:
					// FIXME: CloseEvent
					if ( Asset is not null )
					{
						OpenFiles.Add( Asset.AbsolutePath, this );
					}
					return false;
			}
		}

		Reset();

		UpdateWindowDressing();
		UpdateMenuBar();

		return true;
	}

	public void Save()
	{
		if ( Asset is null )
		{
			SaveAs();
			return;
		}

		WriteAsset();
	}

	public void SaveAs()
	{
		var filter = Graph.GraphType switch
		{
			GraphType.Mix => Util.GetSaveMixGraphFilter(),
			GraphType.Effect => Util.GetSaveEffectGraphFilter(),
			_ => throw new Exception( "Unknown graph type" )
		};

		var extension = Graph.GraphType switch
		{
			GraphType.Mix => MixGraphResource.FileExtension,
			GraphType.Effect => EffectGraphResource.FileExtension,
			_ => throw new Exception( "Unknown graph type" )
		};

		var fd = new FileDialog( this );

		fd.Title = $"Save As";


		fd.SetNameFilter( filter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			Asset = AssetSystem.CreateResource( extension, fd.SelectedFile );
			OpenFiles.Add( Asset.AbsolutePath, this );
			WriteAsset();
		}
	}

	public void EditUndo()
	{
		GraphView?.GraphUndo();
	}

	public void EditRedo()
	{
		GraphView?.GraphRedo();
	}

	public void EditCut()
	{
		GraphView?.GraphCut();
	}

	public void EditCopy()
	{
		GraphView?.GraphCopy();
	}

	public void EditPaste()
	{
		GraphView?.GraphPaste();
	}

	public void EditDelete()
	{
		GraphView?.GraphDelete();
	}

	public void RecenterGraphView()
	{
		GraphView?.Recenter();
	}
}
