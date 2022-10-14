using Tools;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

using SandMix.Tool.NodeGraph;

namespace SandMix.Tool.Widgets;

public class InspectorWidget : DockWidget
{
	Widget Editor;
	InspectorHeader Header;
	NodeUI CurrentNodeUI;

	public string CurrentTime => System.DateTime.Now.ToString();


	public InspectorWidget( Widget parent = null ) : base( "Inspector", "manage_search", parent )
	{
		Widget = new Widget( this );

		Widget.SetLayout( LayoutMode.TopToBottom );

		Header = new InspectorHeader( this );

		Widget.Layout.Add( Header );

		Editor = new Widget( Widget );
		Editor.SetLayout( LayoutMode.TopToBottom );

		Widget.Layout.Add( Editor, -1 );

		StartInspecting( null );
	}

	public override void ChildValuesChanged( Widget source )
	{
		CurrentNodeUI.Graph.CallGraphUpdated();
	}

	public void StartInspecting( NodeUI nodeUI )
	{
		StartInspecting( nodeUI, true );
	}

	public void StartInspecting( NodeUI nodeUI, bool addToHistory = true )
	{
		using var sx = SuspendUpdates.For( this );

		Editor.DestroyChildren();

		CurrentNodeUI = nodeUI;
		var node = nodeUI?.Node;

		var customeditor = CanEditAttribute.CreateEditorForObject( node );
		if ( customeditor != null )
		{
			Editor.Layout.Add( customeditor, 1 );
		}
		else
		{
			var PropertySheet = new Tools.PropertySheet( this );
			PropertySheet.Target = node;

			var scroller = new ScrollArea( this );
			scroller.Canvas = PropertySheet;

			Editor.Layout.Add( scroller );
		}

		if ( addToHistory )
		{
			// Clear everything in our forward
			while ( ObjectHistory.Count > HistoryPlace + 1 )
				ObjectHistory.RemoveAt( ObjectHistory.Count - 1 );

			if ( ObjectHistory.ElementAtOrDefault( HistoryPlace ) != nodeUI )
			{
				// Add to history
				ObjectHistory.Add( nodeUI );
				HistoryPlace = ObjectHistory.Count - 1;

				// limit history size
				if ( ObjectHistory.Count > 100 )
				{
					ObjectHistory.RemoveAt( 0 );
					HistoryPlace--;
				}
			}
		}

		var display = DisplayInfo.ForType( node?.GetType() );
		Header.title.Text = display.Name ?? "";

		// keep buttons updates
		UpdateBackForward();
	}

	private void JumpToHistory()
	{
		StartInspecting( ObjectHistory[HistoryPlace], false );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		if ( e.Button == MouseButtons.Back && GoBack() )
		{
			e.Accepted = true;
			return;
		}

		if ( e.Button == MouseButtons.Forward && GoForward() )
		{
			e.Accepted = true;
			return;
		}

		base.OnMousePress( e );
	}

	public bool GoBack()
	{
		if ( HistoryPlace == 0 )
			return false;

		HistoryPlace--;
		JumpToHistory();

		return true;
	}

	public bool GoForward()
	{
		if ( HistoryPlace >= ObjectHistory.Count() - 1 )
			return false;

		HistoryPlace++;
		JumpToHistory();

		return true;
	}

	void UpdateBackForward()
	{
		Header.UpdateBackForward( HistoryPlace > 0, HistoryPlace < ObjectHistory.Count() - 1 );
	}

	public int HistoryPlace = 0;
	public List<NodeUI> ObjectHistory = new();
}

public class InspectorHeader : ToolBar
{
	InspectorWidget Inspector;

	Option Back;
	Option Forward;
	
	public Label title;

	public InspectorHeader( InspectorWidget parent ) : base( parent.Widget )
	{
		Inspector = parent;

		SetIconSize( 16 );

		Back = AddOption( new Option( this, "Previous", "arrow_back", () => Inspector.GoBack() ) );
		Forward = AddOption( new Option( this, "Next", "arrow_forward", () => Inspector.GoForward() ) );

		AddSeparator();

		//AddWidget( new LineEdit( this ) { PlaceholderText = "Filter.." } );

		title = new Label( this );
		title.Text = "";
		title.Alignment = TextFlag.CenterVertically;
		AddWidget( title );

		UpdateBackForward( false, false );
	}

	public void UpdateBackForward( bool back, bool forward )
	{
		Back.Enabled = back;
		Forward.Enabled = forward;
	}
}
