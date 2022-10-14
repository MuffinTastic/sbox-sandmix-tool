using System;
using System.Collections.Generic;
using System.IO;
using Tools;
using Sandbox;
using SandMix.Nodes;
using SandMix.Tool.Dialogs;
using SandMix.Tool.NodeGraph;

namespace SandMix.Tool.Widgets;

public abstract class NodeGraphWidget : DockWidget
{
	public virtual string DefaultTitle { get; private set; }
	public virtual int NewWidgetNumber { get; }
	public virtual string FileExtension { get; }
	public virtual string SaveFilter { get; }

	internal GraphView GraphView => Widget as GraphView;

	public Asset Asset { get; private set; } = null;

	public bool Changed { get; private set; } = false;
	public bool AttemptSave { get; private set; } = true;
	public string UnchangedTitle { get; private set; }

	public event Action<NodeGraphWidget> NodeGraphFocus;
	public event Action<NodeGraphWidget> NodeGraphClose;

	public NodeGraphWidget( GraphType graphType, string defaultTitle, string icon, Widget parent = null ) : base( defaultTitle, icon, parent )
	{
		DefaultTitle = defaultTitle;
		UnchangedTitle = DefaultTitle;

		DeleteOnClose = true;

		CreateUI( graphType );
	}

	private void CreateUI( GraphType graphType )
	{
		// Widget = new GraphView( graphType, this );

		AddNodeTypes();

		GraphView.GraphUpdated += () => {
			Changed = true;
			UpdateTitle();
		};

		GraphView.NodeSelect += ( _, _ ) => NodeGraphFocus( this );
		GraphView.OnSelectionChanged += () => NodeGraphFocus( this );

		UpdateTitle();
	}

	[Event.Hotload]
	private void AddNodeTypes()
	{
		GraphView.ClearNodeTypes();

		foreach ( var nodeType in NodeTypes )
		{
			GraphView.AddNodeType( nodeType.TargetType );
		}
	}

	protected virtual IEnumerable<TypeDescription> NodeTypes { get; }

	private void UpdateTitle()
	{
		var title = $"{DefaultTitle} {NewWidgetNumber}";

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

		WriteAsset();
	}

	public void SaveAs()
	{
		var fd = new FileDialog( this );

		fd.Title = $"Save As";
		fd.SetNameFilter( SaveFilter );
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			Asset = AssetSystem.CreateResource( FileExtension, fd.SelectedFile );
			WriteAsset();
		}
	}

	public void DontAttemptSave()
	{
		AttemptSave = false;
	}

	public void ReadAsset( Asset asset )
	{
		Asset = asset;

		ReadAssetImpl();

		Changed = false;
		UpdateTitle();
	}

	protected abstract void ReadAssetImpl();

	public void WriteAsset()
	{
		if ( Asset is null )
			return;

		WriteAssetImpl();

		Changed = false;
		UpdateTitle();
	}

	protected abstract void WriteAssetImpl();

	protected override void OnVisibilityChanged( bool visible )
	{
		if ( visible )
			return;

		if ( !Changed || !AttemptSave )
		{
			//Destroy();
			return;
		}

		/*var prompt = new SaveDialog( new[] { this } );
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
		*/

		Show();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		NodeGraphClose( this );
		GraphView.Graph = null;
	}

	public void GraphUndo()
	{
		GraphView.GraphUndo();
	}

	public void GraphRedo()
	{
		GraphView.GraphRedo();
	}

	public void GraphCut()
	{
		GraphView.GraphCut();
	}

	public void GraphCopy()
	{
		GraphView.GraphCopy();
	}

	public void GraphPaste()
	{
		GraphView.GraphPaste();
	}

	internal void GraphDelete()
	{
		GraphView.GraphDelete();
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
