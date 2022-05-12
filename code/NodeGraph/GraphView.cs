using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMixTool.NodeEditor;

public class GraphView : GraphicsView
{
	Graph _graph;
	public Graph Graph
	{
		get => _graph;
		set
		{
			if ( _graph == value ) return;

			_graph = value;
			RebuildFromGraph();
		}
	}

	List<Type> AvailableNodes = new();
	List<Connection> Connections = new();

	public GraphView( Widget parent ) : base( parent )
	{
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
		SetHandleConfig( typeof( string ), new HandleConfig { Color = Color.Parse( "#9bc9c4" ).Value, Icon = "s", Name = "string" } );
		SetHandleConfig( typeof( bool ), new HandleConfig { Color = Color.Parse( "#b49dc9" ).Value, Icon = "b", Name = "bool" } );
	}

	protected override void OnWheel( WheelEvent e )
	{
		Zoom( e.Delta > 0 ? 1.1f : 0.90f, e.Position );
		e.Accept();
	}

	internal void AddNodeType<T>( bool v )
	{
		if ( AvailableNodes.Contains( typeof( T ) ) ) return;

		AvailableNodes.Add( typeof( T ) );
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{
		var clickPos = ToScene( e.LocalPosition );

		var menu = new Menu( this );

		foreach ( var node in AvailableNodes )
		{
			var display = DisplayInfo.ForType( node );

			var action = menu.AddOption( display.Name, display.Icon );
			action.StatusText = display.Description;
			action.Triggered += () => Add( new NodeUI( this, Activator.CreateInstance( node ) as BaseNode ) { Position = clickPos } );
		}

		menu.OpenAt( e.ScreenPosition );
	}

	Vector2 lastMousePosition;

	protected override void OnMouseMove( MouseEvent e )
	{
		if ( e.ButtonState.HasFlag( MouseButtons.Middle ) ) // or space down?
		{
			var delta = ToScene( e.LocalPosition ) - lastMousePosition;
			Translate( delta );
			e.Accepted = true;
			Cursor = CursorShape.ClosedHand;
		}
		else
		{
			Cursor = CursorShape.None;
		}

		e.Accepted = true;
		lastMousePosition = ToScene( e.LocalPosition );
	}

	internal PlugIn DropTarget { get; set; }

	Connection Preview;

	internal void DraggingOutput( NodeUI node, NodeEditor.PlugOut nodeOutput, Vector2 scenePosition, Connection source )
	{
		var item = GetItemAt( scenePosition );

		DropTarget?.Update();
		DropTarget = item as NodeEditor.PlugIn;
		DropTarget?.Update();

		if ( Preview == null )
		{
			Preview = new Connection( nodeOutput );
			Add( Preview );
		}

		Preview.LayoutForPreview( nodeOutput, scenePosition, DropTarget );
	}

	internal void DroppedOutput( NodeUI node, NodeEditor.PlugOut nodeOutput, Vector2 scenePosition, Connection source )
	{
		if ( source != null )
		{
			// Dropped on the same connection it was already connected to
			if ( source.Input == DropTarget ) return;

			source.Disconnect();
			source.Destroy();
		}

		if ( DropTarget != null )
		{
			CreateConnection( nodeOutput, DropTarget );
		}

		Preview?.Destroy();
		Preview = null;

		DropTarget?.Update();
		DropTarget = null;
	}

	private void CreateConnection( PlugOut nodeOutput, PlugIn dropTarget )
	{
		System.ArgumentNullException.ThrowIfNull( nodeOutput );
		System.ArgumentNullException.ThrowIfNull( dropTarget );

		var connection = new Connection( nodeOutput, dropTarget );
		connection.Layout();
		Add( connection );

		Connections.Add( connection );
	}

	internal void RemoveConnection( Connection c )
	{
		Connections.Remove( c );
	}

	internal void NodePositionChanged( NodeUI node )
	{
		foreach ( var connection in Connections )
		{
			if ( !connection.IsAttachedTo( node ) )
				continue;

			connection.Layout();
		}
	}

	void RebuildFromGraph()
	{
		DeleteAllItems();

		foreach ( var item in _graph.Nodes )
		{
			Add( new NodeUI( this, item ) );
		}

		foreach ( var connection in _graph.Connections )
		{
			var o = FindPlugOut( connection.Item2 );
			if ( o == null )
			{
				Log.Warning( $"Couldn't find output {connection.Item2}" );
				continue;
			}

			var i = FindPlugIn( connection.Item1 );
			if ( i == null )
			{
				Log.Warning( $"Couldn't find output {connection.Item1}" );
				continue;
			}

			if ( o == null || i == null )
				continue;

			CreateConnection( o, i );
		}
	}

	public void SetHandleConfig( System.Type t, HandleConfig config )
	{
		handles[t] = config;
	}

	Dictionary<System.Type, HandleConfig> handles = new();

	public HandleConfig GetHandleConfig( System.Type t )
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

	public PlugOut FindPlugOut( string name )
	{
		var split = name.Split( '.', 2 );
		var node = Items.OfType<NodeUI>().FirstOrDefault( x => x.Node.IsNamed( split[0] ) );
		if ( node == null ) return null;
		return node.Outputs.FirstOrDefault( x => x.IsNamed( split[1] ) );
	}

	public PlugIn FindPlugIn( string name )
	{
		var split = name.Split( '.', 2 );
		var node = Items.OfType<NodeUI>().FirstOrDefault( x => x.Node.IsNamed( split[0] ) );
		if ( node == null ) return null;
		return node.Inputs.FirstOrDefault( x => x.IsNamed( split[1] ) );
	}
}
