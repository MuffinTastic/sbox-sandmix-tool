using Tools;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

using SandMix.Tool.NodeGraph;
using SandMix.Tool.Inspector;

namespace SandMix.Tool.Widgets;

public class InspectorWidget : DockWidget
{
	Widget Editor;
	InspectorHeader Header;
	NodeUI CurrentNodeUI;

	ScrollArea PropertyScroller;
	Separator Separator;
	Widget NodeEditor;

	public string CurrentTime => System.DateTime.Now.ToString();


	public InspectorWidget( Widget parent = null ) : base( "Inspector", "manage_search", parent )
	{
		Widget = new Widget( this );

		Widget.SetLayout( LayoutMode.TopToBottom );

		Header = new InspectorHeader( this );

		Widget.Layout.Add( Header );

		Editor = new Widget( Widget );
		Editor.SetLayout( LayoutMode.LeftToRight );

		Widget.Layout.Add( Editor, -1 );

		StartInspecting( null );
	}

	public override void ChildValuesChanged( Widget source )
	{
		CurrentNodeUI?.Graph?.CallGraphUpdated();
		CurrentNodeUI?.Update();
	}

	public void StartInspecting( NodeUI nodeUI )
	{
		StartInspecting( nodeUI, true );
	}

	public void StartInspecting( NodeUI nodeUI, bool addToHistory = true )
	{
		using var sx = SuspendUpdates.For( this );

		Editor.DestroyChildren();
		Editor.Layout.Clear( true );

		CurrentNodeUI = nodeUI;
		var node = nodeUI?.Node;

		if (node is not null)
		{
			var PropertySheet = new Inspector.PropertySheet( this );
			PropertySheet.Target = node;
			PropertySheet.OnChildValuesChanged += ChildValuesChanged;

			PropertyScroller = new ScrollArea( this );
			PropertyScroller.Canvas = PropertySheet;

			PropertyScroller.MinimumWidth = 400.0f;
			PropertyScroller.MaximumWidth = 400.0f;

			Editor.Layout.Add( PropertyScroller, 0 );

			NodeEditor = SandMixInspectorAttribute.CreateEditorForObject( node );
			if ( NodeEditor is null )
			{
				NodeEditor = new Widget( null );
			}

			Separator = Editor.Layout.AddSeparator();
			Editor.Layout.Add( NodeEditor, 1 );
		}

		MinimumSize = Editor.MinimumSize = new Vector2(400.0f, 300.0f);

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

	protected override void DoLayout()
	{
		if ( CurrentNodeUI is null )
		{
			return;
		}

		if ( NodeEditor != null )
		{
			if ( Size.x < PropertyScroller.Size.x * 2.0f )
			{
				Separator.Hide();
				NodeEditor.Hide();
			}
			else
			{

				Separator.Show();
				NodeEditor.Show();
			}
		}
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
