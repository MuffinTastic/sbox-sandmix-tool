﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Tools;
using SandMix.Nodes;
using SandMix.Nodes.Mix;
using SandMix.Nodes.Effects;

namespace SandMix.Tool.NodeGraph;

public class GraphView : GraphicsView
{
	GraphContainer _graph;
	public GraphContainer Graph
	{
		get => _graph;
		set
		{
			if ( _graph == value ) return;

			_graph = value;
			RebuildAllFromMainGraph();
		}
	}

	List<Type> AvailableNodes = new();

	List<NodeUI> Nodes = new();
	List<ConnectionUI> Connections = new();

	public event Action<NodeUI, bool> NodeSelect;
	public event Action GraphUpdated;

	public GraphView( GraphContainer graph, Widget parent ) : base( parent )
	{
		Assert.NotNull( graph );

		Antialiasing = true;
		TextAntialiasing = true;
		BilinearFiltering = true;

		SceneRect = new Rect( -100000, -100000, 200000, 200000 );

		HorizontalScrollbar = ScrollbarMode.Off;
		VerticalScrollbar = ScrollbarMode.Off;
		MouseTracking = true;

		SetHandleConfig( typeof( int ), new HandleConfig { Color = Color.Parse( "#82ba6d" ).Value, Icon = "i", Name = "Integer" } );
		SetHandleConfig( typeof( float ), new HandleConfig { Color = Color.Parse( "#be9363" ).Value, Icon = "f", Name = "Float" } );
		SetHandleConfig( typeof( Vector3 ), new HandleConfig { Color = Color.Parse( "#fff08a" ).Value, Icon = "v", Name = "Vector3" } );
		SetHandleConfig( typeof( bool ), new HandleConfig { Color = Color.Parse( "#b49dc9" ).Value, Icon = "b", Name = "Boolean" } );
		SetHandleConfig( typeof( float[][] ), new HandleConfig { Color = Color.Parse( "#9dc2d5" ).Value, Icon = "a", Name = "Audio" } );

		Graph = graph;

		SetViewport( center: Graph.EditorCenter, scale: Graph.EditorScale );
		SetupNodeTypes();

		Show();
	}

	public void UpdateLanguage()
	{
		if ( !IsValid )
		{
			return;
		}
	
		foreach ( var node in Nodes )
		{
			node.UpdateLanguage();
		}

		Update();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		NodeSelect = null;
		GraphUpdated = null;
	}

	public void SetViewport( Vector2 center, float scale )
	{
		Graph.EditorCenter = center;
		Graph.EditorScale = scale;

		Scale = (Vector2) Graph.EditorScale;
		CenterOn( Graph.EditorCenter );
	}

	public void Recenter()
	{
		SetViewport( center: Vector2.Zero, scale: 1.0f );
	}

	protected override void OnWheel( WheelEvent e )
	{
		float delta = e.Delta > 0 ? 1.1f : 0.90f;
		Graph.EditorScale *= delta;
		Zoom( delta, e.Position );
		e.Accept();

		CallGraphUpdated();
	}

	internal void AddNodeType<T>()
	{
		AddNodeType( typeof( T ) );
	}

	internal void AddNodeType( Type t )
	{
		if ( AvailableNodes.Contains( t ) ) return;

		AvailableNodes.Add( t );
	}

	internal void ClearNodeTypes()
	{
		AvailableNodes.Clear();
	}

	[Event.Hotload]
	private void SetupNodeTypes()
	{
		ClearNodeTypes();

		var baseNodeType = Graph.GraphType switch
		{
			GraphType.Mix => typeof( BaseMixGraphNode ),
			GraphType.Effect => typeof( BaseEffectGraphNode ),
			_ => throw new Exception( "Unknown graph type" )
		};

		var descriptions =
			TypeLibrary.GetDescriptions( baseNodeType )
			.Where( td => !td.IsAbstract );

		foreach ( var nodeType in descriptions )
		{
			AddNodeType( nodeType.TargetType );
		}
	}

	private class ContextGroup
	{
		public string Name;
		public List<ContextItem> Items;
	}

	private class ContextItem
	{
		public DisplayInfo DisplayInfo;
		public Action Action;
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{
		var clickPos = ToScene( e.LocalPosition );

		var groups = new List<ContextGroup>();

		// Add all nodes
		foreach ( var node in AvailableNodes )
		{
			var display = DisplayInfo.ForType( node );

			if ( !SandMixTool.Debug && display.Group == "#smix.node.category.debug" )
			{
				continue;
			}

			var groupName = Util.GetLocalized( display.Group ?? "#smix.node.category.other" );

			var group = groups.Where( g => g.Name == groupName ).FirstOrDefault();
			if ( group is null )
			{
				group = new ContextGroup
				{
					Name = groupName,
					Items = new List<ContextItem>()
				};
				groups.Add( group );
			}

			var item = new ContextItem
			{
				DisplayInfo = display,
				Action = () => CreateNode( new NodeUI( this, Activator.CreateInstance( node ) as BaseNode ) { Position = clickPos }, addToUndo: true )
			};

			group.Items.Add( item );
		}

		var menu = new Menu( this );

		// Sort and add the nodes
		groups.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

		foreach ( var group in groups )
		{
			group.Items.Sort( ( a, b ) => a.DisplayInfo.Name.CompareTo( b.DisplayInfo.Name ) );

			var subMenu = menu.AddMenu( group.Name );

			foreach ( var item in group.Items )
			{
				var action = subMenu.AddOption( Util.GetLocalized( item.DisplayInfo.Name ), item.DisplayInfo.Icon );
				action.StatusText = Util.GetLocalized( item.DisplayInfo.Description );
				action.Triggered = item.Action;
			}
		}

		var rightClickedItem = GetItemAt( clickPos );

		if ( rightClickedItem is not null && rightClickedItem is not PlugUI )
		{
			if ( !SelectedItems.Any() )
			{
				rightClickedItem.Selected = true;
			}
		}

		if ( SelectedItems.Any() )
		{
			menu.AddSeparator();
			var action = menu.AddOption( Util.GetLocalized( "#smix.ui.graphview.deleteselection" ) );
			action.Triggered = GraphDelete;
		}

		menu.OpenAt( e.ScreenPosition );
	}

	private const float GridSize = 250.0f;
	private const float GridLineWidth = 3.0f;

	protected override void OnPaint()
	{
		var area = GetVisibleArea();

		float lineWidth = GridLineWidth * Graph.EditorScale;

		void DrawGrid( Vector2 offset )
		{
			for ( float x = ((area.Left - offset.x) / GridSize).Floor() * GridSize + offset.x; x < area.Right; x += GridSize )
			{
				Paint.DrawLine( new Vector2[] {
					FromScene( new Vector2( x, area.Top ) ),
					FromScene( new Vector2( x, area.Bottom ) ),
				} );
			}

			for ( float y = ((area.Top - offset.y) / GridSize).Floor() * GridSize + offset.y; y < area.Bottom; y += GridSize )
			{
				Paint.DrawLine( new Vector2[] {
					FromScene( new Vector2( area.Left, y ) ),
					FromScene( new Vector2( area.Right, y ) ),
				} );
			}
		}

		Paint.SetPen( Theme.Grey * 0.45f, lineWidth );

		DrawGrid( (Vector2) GridSize / 2.0f );

		Paint.SetPen( Theme.Grey * 0.6f, lineWidth );

		DrawGrid( Vector2.Zero );

		base.OnPaint();
	}

	Vector2 dragStart;
	Vector2 lastMousePosition;

	Selection SelectionBox;

	protected override void OnMouseMove( MouseEvent e )
	{
		if ( e.ButtonState.HasFlag( MouseButtons.Middle ) ) // or space down?
		{
			var delta = ToScene( e.LocalPosition ) - lastMousePosition;
			Translate( delta );
			Graph.EditorCenter = GetVisibleArea().Center;
			e.Accepted = true;
			Cursor = CursorShape.ClosedHand;

			CallGraphUpdated();
		}
		else
		{
			Cursor = CursorShape.None;
		}

		e.Accepted = true;
		lastMousePosition = ToScene( e.LocalPosition );

		if ( SelectionBox is not null )
		{
			SelectionBox.UpdateSelection( dragStart, lastMousePosition );
		}
	}

	NodeUI lastInspected;

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		var item = GetItemAt( ToScene( e.LocalPosition ) );
		var node = item as NodeUI;
		node ??= (item as PlugUI)?.Node;

		NodeSelect?.Invoke( node, node is not null );
		lastInspected = node;

		if ( item is null && e.LeftMouseButton )
		{
			dragStart = ToScene( e.LocalPosition );
			SelectionBox = new Selection( this );
			SelectionBox.UpdateSelection( dragStart, lastMousePosition );
			Add( SelectionBox );
		}
	}

	protected override void OnMouseReleased( MouseEvent e )
	{
		base.OnMouseReleased( e );

		if ( SelectionBox is not null )
		{
			SelectionBox.Destroy();
			SelectionBox = null;
		}
	}

	public bool HasSelection()
	{
		return SelectedItems.Any();
	}

	internal PlugIn DropTarget { get; set; }

	ConnectionUI Preview;

	internal void DraggingOutput( NodeUI node, NodeGraph.PlugOut nodeOutput, Vector2 scenePosition, ConnectionUI source )
	{
		var item = GetItemAt( scenePosition );

		DropTarget?.Update();
		DropTarget = item as NodeGraph.PlugIn;
		if ( DropTarget is not null )
		{
			var OutputType = nodeOutput.Property.PropertyType;
			var InputType = DropTarget.Property.PropertyType;

			var otherConnected = Connections
				.Where( c => c.Input == DropTarget && c != source )
				.FirstOrDefault();

			if ( nodeOutput.Node == DropTarget.Node ||  // Same node as was selected before
				OutputType != InputType ||              // Incompatible type
				otherConnected is not null )            // Other nodes are already connected
			{
				DropTarget = null;
			}
		}
		DropTarget?.Update();

		if ( Preview == null )
		{
			Preview = new ConnectionUI( nodeOutput );
			Add( Preview );
		}

		Preview.LayoutForPreview( nodeOutput, scenePosition, DropTarget );
	}

	internal void DroppedOutput( NodeUI node, NodeGraph.PlugOut nodeOutput, Vector2 scenePosition, ConnectionUI source )
	{
		try
		{
			if ( source != null )
			{
				// Dropped on the same connection it was already connected to
				if ( source.Input == DropTarget ) return;

				source.Disconnect();
				source.Destroy();

				CallGraphUpdated();
			}

			if ( DropTarget != null )
			{
				CreateConnection( nodeOutput, DropTarget, addToUndo: true );
			}
		}
		finally
		{
			Preview?.Destroy();
			Preview = null;

			DropTarget?.Update();
			DropTarget = null;
		}
	}

	public void UnfocusAllNodes()
	{
		foreach ( var node in Nodes )
		{
			node.Selected = false;
		}
	}

	private void CreateNode( NodeUI node, bool addToUndo = false )
	{
		Nodes.Add( node );
		Add( node );
		Graph?.Add( node.Node );

		if ( SandMixTool.Debug )
			Log.Info( $"Created {node.Node.GetType().Name} node {node.Node.Identifier}" );

		if (addToUndo)
		{
			var undoState = new GraphChange();
			undoState.Creation = true;
			undoState.Graph = new GraphContainer( Graph.GraphType );
			undoState.Graph.Add( node.Node );
			UndoStates.Push( undoState );
			RedoStates.Clear();
		}

		CallGraphUpdated();
	}

	private void RemoveNode( NodeUI node, bool addToUndo = false )
	{
		var badConnections = Connections.Where( c => c.IsAttachedTo( node ) ).ToList();
		foreach ( var connection in badConnections )
		{
			connection.Disconnect();
		}

		Nodes.Remove( node );
		Graph?.Remove( node.Node );
		node.Destroy();

		if ( SandMixTool.Debug )
			Log.Info( $"Removed {node.Node.GetType().Name} node {node.Node.Identifier}" );

		if ( addToUndo )
		{
			var undoState = new GraphChange();
			undoState.Creation = false;
			undoState.Graph = new GraphContainer( Graph.GraphType );
			undoState.Graph.Add( node.Node );
			UndoStates.Push( undoState );
			RedoStates.Clear();
		}

		CallGraphUpdated();
	}

	private void CreateConnection( PlugOut nodeOutput, PlugIn dropTarget, bool addToUndo = false )
	{
		System.ArgumentNullException.ThrowIfNull( nodeOutput );
		System.ArgumentNullException.ThrowIfNull( dropTarget );

		var connection = new ConnectionUI( nodeOutput, dropTarget );
		connection.Layout();
		Add( connection );

		Connections.Add( connection );
		Graph?.Connect( nodeOutput.Identifier, dropTarget.Identifier );

		if ( SandMixTool.Debug )
			Log.Info( $"Created connection {nodeOutput.Identifier} → {dropTarget.Identifier}" );

		if ( addToUndo )
		{
			var undoState = new GraphChange();
			undoState.Creation = true;
			undoState.Graph = new GraphContainer( Graph.GraphType );
			undoState.Graph.Connect( nodeOutput.Identifier, dropTarget.Identifier );
			UndoStates.Push( undoState );
			RedoStates.Clear();
		}

		CallGraphUpdated();
	}

	internal void RemoveConnection( ConnectionUI connection, bool addToUndo = false )
	{
		Connections.Remove( connection );
		Graph?.Disconnect( connection.Output.Identifier, connection.Input.Identifier );
		connection.Destroy();

		if ( SandMixTool.Debug )
			Log.Info( $"Removed connection {connection.Output.Identifier} → {connection.Input.Identifier}" );

		if ( addToUndo )
		{
			var undoState = new GraphChange();
			undoState.Creation = false;
			undoState.Graph = new GraphContainer( Graph.GraphType );
			undoState.Graph.Connect( connection.Output.Identifier, connection.Input.Identifier );
			UndoStates.Push( undoState );
			RedoStates.Clear();
		}

		CallGraphUpdated();
	}

	internal void NodePositionChanged( NodeUI node )
	{
		foreach ( var connection in Connections )
		{
			if ( !connection.IsAttachedTo( node ) )
				continue;

			connection.Layout();
		}

		CallGraphUpdated();
	}

	public void CallGraphUpdated()
	{
		GraphUpdated?.Invoke();
	}

	Rect GetVisibleArea()
	{
		var topLeft = ToScene( 0 );
		var bottomRight = ToScene( Size );
		return new Rect( topLeft, bottomRight - topLeft );
	}

	void BuildFromGraph( GraphContainer graph, bool paste = false )
	{
		if ( graph is null )
			return;

		var pastedNodes = new List<NodeUI>();

		foreach ( var node in graph.Nodes )
		{
			var nodeUI = new NodeUI( this, node );
			CreateNode( nodeUI );

			if ( paste )
				pastedNodes.Add( nodeUI );
		}

		if ( pastedNodes.Any() )
		{
			foreach ( var item in SelectedItems )
			{
				item.Selected = false;
			}

			// guaranteed to have at least one
			var firstNodeCenter = pastedNodes.First().SceneRect.Center;
			var topLeft = firstNodeCenter;
			var bottomRight = firstNodeCenter;
			
			foreach ( var node in pastedNodes )
			{
				topLeft = topLeft.ComponentMin( node.SceneRect.Center );
				bottomRight = bottomRight.ComponentMax( node.SceneRect.Center );
			}

			var areaSize = bottomRight - topLeft;
			var nodesCenter = topLeft + areaSize / 2;

			var visibleArea = GetVisibleArea();
			Vector2 target = IsUnderMouse ? lastMousePosition : visibleArea.Center;

			var adjustToCenter = nodesCenter - target;


			// now actually offset them
			foreach ( var node in pastedNodes )
			{
				node.Position -= adjustToCenter;
				node.Selected = true;

				if ( SandMixTool.Debug )
					Log.Info( $"pasted node {node.Node.Identifier}" );
			}
		}

		foreach ( var connection in graph.Connections )
		{
			if ( SandMixTool.Debug && paste )
				Log.Info( $"Pasted connection {connection}" );

			var o = FindPlugOut( connection.Item1 );
			if ( o == null )
			{
				Log.Warning( $"Couldn't find output {connection.Item1}" );
				continue;
			}

			var i = FindPlugIn( connection.Item2 );
			if ( i == null )
			{
				Log.Warning( $"Couldn't find input {connection.Item2}" );
				continue;
			}

			if ( o == null || i == null )
				continue;

			CreateConnection( o, i );
		}
	}

	void RebuildAllFromMainGraph()
	{
		DeleteAllItems();
		BuildFromGraph( _graph );
	}

	public void SetHandleConfig( Type t, HandleConfig config )
	{
		handles[t] = config;
	}

	Dictionary<Type, HandleConfig> handles = new();

	public HandleConfig GetHandleConfig( Type t )
	{
		if ( handles.TryGetValue( t, out HandleConfig config ) )
			return config;

		return new HandleConfig
		{
			Name = t.Name,
			Color = Color.Parse( "#ddd" ).Value,
			Icon = "people"
		};
	}

	public PlugOut FindPlugOut( string id )
	{
		var split = id.Split( '.', 2 );
		var node = Items.OfType<NodeUI>().FirstOrDefault( x => x.Node.HasIdentifier( split[0] ) );
		if ( node == null ) return null;
		return node.Outputs.FirstOrDefault( x => x.IsNamed( split[1] ) );
	}

	public PlugIn FindPlugIn( string id )
	{
		var split = id.Split( '.', 2 );
		var node = Items.OfType<NodeUI>().FirstOrDefault( x => x.Node.HasIdentifier( split[0] ) );
		if ( node == null ) return null;
		return node.Inputs.FirstOrDefault( x => x.IsNamed( split[1] ) );
	}

	// Shortcut events

	Stack<GraphChange> UndoStates = new();
	Stack<GraphChange> RedoStates = new();

	public void ApplyGraphChange( GraphChange change )
	{
		if ( change.Creation )
		{
			// it's being annoying so let's go off of identifiers
			var changeIdents = change.Graph.Nodes.Select( n => n.Identifier );

			Log.Trace( Nodes.Count );

			foreach ( var node in Nodes.Where( n => changeIdents.Contains( n.Node.Identifier ) ).ToList() )
			{
				RemoveNode( node );
			}

			foreach ( var connection in change.Graph.Connections )
			{
				var connUI = Connections.Find( c =>
					c.Output.Identifier == connection.Item1 &&
					c.Input.Identifier == connection.Item2
				);

				if ( connUI is null )
				{
					continue;
				}

				RemoveConnection( connUI );
			}
		}
		else
		{
			BuildFromGraph( change.Graph );
		}
	}

	public void GraphUndo()
	{
		if ( !CanUndo() )
			return;

		var state = UndoStates.Pop();

		ApplyGraphChange( state );

		state.Creation = !state.Creation;
		RedoStates.Push( state );

		CallGraphUpdated();

		Focus();
	}

	public void GraphRedo()
	{
		if ( !CanRedo() )
			return;

		var state = RedoStates.Pop();
		
		ApplyGraphChange( state );

		state.Creation = !state.Creation;
		UndoStates.Push( state );

		CallGraphUpdated();

		Focus();
	}

	public void GraphCut()
	{
		GraphCopy();
		GraphDelete();
		Focus();
	}

	public void GraphCopy()
	{
		var copyGraph = new GraphContainer( Graph.GraphType );

		// Only nodes get selected so we'll have to find the connections ourselves
		//                     But let's be sure V
		copyGraph.Nodes = SelectedItems.OfType<NodeUI>().Select( n => n.Node ).ToList();

		foreach ( var connection in Connections )
		{
			if ( !copyGraph.Nodes.Contains( connection.Output.Node.Node ) )
				continue;

			if ( !copyGraph.Nodes.Contains( connection.Input.Node.Node ) )
				continue;

			copyGraph.Connect( connection.Output.Identifier, connection.Input.Identifier );
		}

		Clipboard.Copy( copyGraph.Serialize() );

		Focus();
	}

	public void GraphPaste()
	{
		var pasteJson = Clipboard.Paste();
		
		try
		{
			var pasteGraph = GraphContainer.Deserialize( pasteJson );

			if ( pasteGraph is not null )
			{
				if (pasteGraph.GraphType != Graph.GraphType)
				{
					return;
				}

				pasteGraph.RegenerateIdentifiers();
				BuildFromGraph( pasteGraph, paste: true );
				
				var undoState = new GraphChange();
				undoState.Creation = true;
				undoState.Graph = pasteGraph;
				UndoStates.Push( undoState );

				NodeSelect?.Invoke( null, false );
			}
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Couldn't paste into mixgraph: {ex.Message}" );
			Log.Warning( pasteJson );
		}

		Focus();
	}

	public void GraphDelete()
	{
		var undoState = new GraphChange();
		undoState.Creation = false;
		undoState.Graph = new GraphContainer( Graph.GraphType );
		
		foreach ( var node in SelectedItems.OfType<NodeUI>().ToList() )
		{
			if ( node == lastInspected )
				NodeSelect?.Invoke( null, false );

			undoState.Graph.Add( node.Node );

			foreach ( var connection in Connections.Where( c => c.IsAttachedTo( node ) ) )
			{
				undoState.Graph.Connect( connection.Output.Identifier, connection.Input.Identifier );
			}

			RemoveNode( node );
		}

		UndoStates.Push( undoState );

		Focus();
	}

	public bool CanUndo()
	{
		return UndoStates.Any();
	}

	public bool CanRedo()
	{
		return RedoStates.Any();
	}
}
