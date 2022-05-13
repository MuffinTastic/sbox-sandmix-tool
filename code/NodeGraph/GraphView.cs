﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMixTool.NodeGraph;

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

			var groupName = display.Group ?? "Other";

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
				Action = () => Add( new NodeUI( this, Activator.CreateInstance( node ) as BaseNode ) { Position = clickPos } )
			};

			group.Items.Add( item );
		}

		var menu = new Menu( this );

		// Sort and add the nodes
		groups.Sort((a, b) => a.Name.CompareTo( b.Name ) );
		
		foreach ( var group in groups )
		{
			group.Items.Sort((a, b) => a.DisplayInfo.Name.CompareTo( b.DisplayInfo.Name ) );
			
			var subMenu = menu.AddMenu( group.Name );

			foreach ( var item in group.Items )
			{
				var action = subMenu.AddOption( item.DisplayInfo.Name, item.DisplayInfo.Icon );
				action.StatusText = item.DisplayInfo.Description;
				action.Triggered = item.Action;
			}
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

	internal void DraggingOutput( NodeUI node, NodeGraph.PlugOut nodeOutput, Vector2 scenePosition, Connection source )
	{
		var item = GetItemAt( scenePosition );

		DropTarget?.Update();
		DropTarget = item as NodeGraph.PlugIn;
		if ( DropTarget is not null )
		{
			var OutputType = nodeOutput.Property.PropertyType;
			var InputType = DropTarget.Property.PropertyType;

			if ( nodeOutput.Node == DropTarget.Node || OutputType != InputType )
			{
				DropTarget = null;
			}
		}
		DropTarget?.Update();

		if ( Preview == null )
		{
			Preview = new Connection( nodeOutput );
			Add( Preview );
		}

		Preview.LayoutForPreview( nodeOutput, scenePosition, DropTarget );
	}

	internal void DroppedOutput( NodeUI node, NodeGraph.PlugOut nodeOutput, Vector2 scenePosition, Connection source )
	{
		try
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
		}
		finally
		{
			Preview?.Destroy();
			Preview = null;

			DropTarget?.Update();
			DropTarget = null;
		}
	}

	public Action<BaseNode, bool> NodeSelect;

	internal void OnNodeSelect( BaseNode node )
	{
		//NodeSelect( node, true );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		var item = GetItemAt( ToScene( e.LocalPosition ) );

		if ( item is NodeUI node )
		{
			NodeSelect( node.Node, true );
		}
		else if ( item is Plug plug )
		{
			NodeSelect( plug.Node.Node, true );
		}
		else
		{
			NodeSelect( null, false );
		}
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
