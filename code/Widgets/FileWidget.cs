using System;
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

namespace SandMix.Tool.Widgets;

public class FileWidget : DockWidget
{
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
		: base( "Graph View", SandMixTool.BaseIcon, parent )
	{
		Preview = preview;
		Inspector = inspector;

		UpdateWindowDressing();
	}

	public void SetMenuBarOptions( ToolWindow.MenuBarOptions menuOptions )
	{
		MenuOptions = menuOptions;
		UpdateMenuBar();
	}

	private void UpdateWindowDressing()
	{
		var window = Parent as ToolWindow;

		if ( Graph is null )
		{
			SetIcon( SandMixTool.BaseIcon );
			window.ResetWindowIcon();
			window.ResetWindowTitle();
			return;
		}

		var icon = Graph.GraphType switch
		{
			GraphType.Mix => MixGraphResource.Icon,
			GraphType.Effect => EffectResource.Icon,
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
				GraphType.Mix => "New Mixgraph",
				GraphType.Effect => "New Effect",
				_ => throw new Exception( "Unknown graph type" )
			};
		}

		if ( UnsavedChanges )
		{
			title += "*";
		}

		SetIcon( icon );
		window.SetWindowIcon( icon );
		window.SetWindowTitle( title );
	}

	private void UpdateMenuBar()
	{
		var fileOpen = GraphView is not null;
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
			fd.SetNameFilter( SandMixTool.FindFileFilter );
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
	}

	public async Task<bool> FileClose()
	{
		if ( UnsavedChanges )
		{
			var result = await SaveDialog.Run( Parent );

			switch ( result )
			{
				case SaveDialog.Result.Yes:
					Save();
					break;

				case SaveDialog.Result.No:
					// continue on to destroy
					break;

				case SaveDialog.Result.Cancel:
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
			GraphType.Mix => SandMixTool.SaveMixGraphFilter,
			GraphType.Effect => SandMixTool.SaveEffectFilter,
			_ => throw new Exception( "Unknown graph type" )
		};

		var extension = Graph.GraphType switch
		{
			GraphType.Mix => MixGraphResource.FileExtension,
			GraphType.Effect => EffectResource.FileExtension,
			_ => throw new Exception( "Unknown graph type" )
		};

		var fd = new FileDialog( this );

		fd.Title = $"Save As";


		fd.SetNameFilter( filter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			Asset = AssetSystem.CreateResource( extension, fd.SelectedFile );
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
		throw new NotImplementedException();
	}
}
