using Sandbox;
using SandMix;
using SandMix.Nodes;
using SandMix.Nodes.Audio;
using SandMix.Nodes.Maths;
using SandMixTool.Dialogs;
using SandMixTool.NodeGraph;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools;

namespace SandMixTool.Widgets;

public class MixGraphWidget : DockWidget
{
	public const string DefaultTitle = "New Mix Graph";
	public static int NewGraphNumber = 0;

	public MixGraphWidget LastOpened { get; private set; }

	private MainWindow MainWindow => Parent as MainWindow;

	internal GraphView GraphView => Widget as GraphView;

	public Asset Asset { get; private set; } = null;
	public MixGraphResource Resource { get; private set; } = null;

	public bool Changed { get; private set; } = false;
	public bool AttemptSave { get; private set; } = true;
	public string UnchangedTitle { get; private set; } = DefaultTitle;

	public event Action<MixGraphWidget> MixGraphFocus;
	public event Action<MixGraphWidget> MixGraphClose;

	public MixGraphWidget( Widget parent = null ) : base( DefaultTitle, "account_tree", parent )
	{
		NewGraphNumber++;

		DeleteOnClose = true;

		CreateUI();
	}

	private void CreateUI()
	{
		Widget = new GraphView( this );

		AddNodeTypes();

		GraphView.GraphUpdated += () => {
			Changed = true;
			UpdateTitle();
		};

		GraphView.NodeSelect += ( _, _ ) => MixGraphFocus( this );
		GraphView.OnSelectionChanged += () => MixGraphFocus( this );

		UpdateTitle();
	}

	[Event.Hotload]
	private void AddNodeTypes()
	{
		GraphView.ClearNodeTypes();

		var allNodeTypes = TypeLibrary.GetDescriptions<BaseNode>().Where( td => !td.IsAbstract );
		foreach ( var nodeType in allNodeTypes )
		{
			GraphView.AddNodeType( nodeType.TargetType );
		}
	}

	private void UpdateTitle()
	{
		var title = $"{DefaultTitle} {NewGraphNumber}";

		if ( Asset is not null )
		{
			title = Path.GetFileName( Asset.AbsolutePath );
		}

		UnchangedTitle = title;

		if ( Changed )
			title += "*";

		Title = title;
	}

	public void Save()
	{
		if ( Asset is null )
		{
			SaveAs();
			return;
		}

		WriteToDisk();
	}

	public void SaveAs()
	{
		var fd = new FileDialog( this );

		fd.Title = $"Save As";
		fd.SetNameFilter( SandMixTool.SaveMixGraphFilter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			Asset = AssetSystem.CreateResource( MixGraphResource.FileExtension, fd.SelectedFile );
			WriteToDisk();
		}
	}

	public void DontAttemptSave()
	{
		AttemptSave = false;
	}

	public void ReadFromDisk( string filePath )
	{
		Asset = AssetSystem.FindByPath( filePath );

		if ( Asset.TryLoadResource<MixGraphResource>( out var resource ) )
		{
			if ( resource.JsonData is not null )
			{
				GraphView.Graph = GraphContainer.Deserialize( resource.JsonData );
			}
		}

		Changed = false;
		UpdateTitle();
	}

	public void WriteToDisk()
	{
		if ( Asset is null )
			return;

		if ( Asset.TryLoadResource<MixGraphResource>( out var resource ) )
		{
			resource.JsonData = GraphView.Graph.Serialize();
			Asset.SaveToDisk( resource );
			Asset.Compile( full: false );
		}

		Changed = false;
		UpdateTitle();
	}

	protected override void OnVisibilityChanged( bool visible )
	{
		if ( visible )
			return;

		if ( !Changed || !AttemptSave )
		{
			//Destroy();
			return;
		}

		var prompt = new SaveDialog( new[] { this } );
		prompt.Triggered += ( result ) =>
		{
			switch ( result )
			{
				case SaveDialog.Result.Yes:
					Save();
					Destroy(); // destroy since close isn't a thing
					break;
				case SaveDialog.Result.No:
					DontAttemptSave();
					Destroy(); // we don't care about the changes, kill it
					break;
				case SaveDialog.Result.Cancel:
					break;
			}
		};

		prompt.Show();

		Show();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		MixGraphClose( this );
		GraphView.Graph = null;
	}

	public void GraphUndo()
	{
		GraphView.OnGraphUndo();
	}

	public void GraphRedo()
	{
		GraphView.OnGraphRedo();
	}

	public void GraphCut()
	{
		GraphView.OnGraphCut();
	}

	public void GraphCopy()
	{
		GraphView.OnGraphCopy();
	}

	public void GraphPaste()
	{
		GraphView.OnGraphPaste();
	}

	internal void GraphDelete()
	{
		GraphView.OnGraphDelete();
	}

	public bool GraphCanUndo()
	{
		return GraphView.CanUndo();
	}

	public bool GraphCanRedo()
	{
		return GraphView.CanRedo();
	}

	public bool GraphHasSelection()
	{
		return GraphView.HasSelection();
	}
}
