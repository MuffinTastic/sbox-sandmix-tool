using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tools;

namespace SandMixTool.NodeEditor;

public partial class NodeUI : Tools.GraphicsItem
{
	public BaseNode Node { get; protected set; }

	public GraphView Graph { get; protected set; }

	public Sandbox.DisplayInfo DisplayInfo { get; set; }

	public Color SelectionOutline = Color.Parse( "#ff99c8" ) ?? default;
	public Color PrimaryColor = Color.Parse( "#ff99c8" ) ?? default;


	public List<PlugIn> Inputs = new();
	public List<PlugOut> Outputs = new();

	float TitleHeight = 30.0f;

	public NodeUI( GraphView graph, BaseNode node )
	{
		Node = node;
		Graph = graph;
		Movable = true;
		Selectable = true;
		HoverEvents = true;

		Size = new Vector2( 256, 512 );
		Position = node.Position;

		DisplayInfo = Sandbox.DisplayInfo.For( node );

		Tooltip = DisplayInfo.Description ?? "No Description";

		foreach ( var property in Node.GetType().GetProperties() )
		{
			var input = property.GetCustomAttribute<BaseNode.InputAttribute>();
			if ( input != null )
			{
				var handle = new PlugIn( this, property, input );
				handle.Parent = this;

				Inputs.Add( handle );
			}

			var output = property.GetCustomAttribute<BaseNode.OutputAttribute>();
			if ( output != null )
			{
				var handle = new PlugOut( this, property, output );
				handle.Parent = this;

				Outputs.Add( handle );
			}
		}

		Layout();
	}

	void Layout()
	{
		var inputHeight = Inputs.Sum( x => x.Size.y );
		var outputHeight = Outputs.Sum( x => x.Size.y );

		var top = TitleHeight;
		var columnWidth = 128.0f;
		var width = columnWidth * 2;

		if ( Inputs.Count == 0 ) width = columnWidth;
		if ( Outputs.Count == 0 ) width = columnWidth;

		foreach ( var input in Inputs )
		{
			input.Position = new Vector2( -4, top );
			input.Size = input.Size.WithX( columnWidth );
			top += input.Size.y;
		}

		top = TitleHeight;
		foreach ( var output in Outputs )
		{
			output.Position = new Vector2( width - columnWidth + 4, top );
			output.Size = output.Size.WithX( columnWidth );
			top += output.Size.y;
		}

		var bodyHeight = MathF.Max( inputHeight, outputHeight );
		Size = new Vector2( width, TitleHeight + bodyHeight );

	}

	protected override void OnPaint()
	{
		var rect = new Rect( 0, Size );//.Contract( 6, 0 );

		PrimaryColor = new Color( 0.7f, 0.7f, 0.7f );
		PrimaryColor = Color.Lerp( PrimaryColor, Theme.Blue, 0.1f );
		if ( Paint.HasSelected ) PrimaryColor = SelectionOutline;
		else if ( Paint.HasMouseOver ) PrimaryColor = Color.Lerp( PrimaryColor, SelectionOutline, 0.5f );

		Paint.SetPen( PrimaryColor.WithAlpha( 0.5f ), 1.0f );
		Paint.SetBrush( PrimaryColor.Darken( 0.7f ).WithAlpha( 0.8f ) );
		Paint.DrawRect( rect, 5.0f );

		Paint.SetPenEmpty();
		Paint.SetBrush( PrimaryColor.WithAlpha( 0.05f ) );

		if ( Inputs.Count > 0 )
		{
			//	Paint.DrawRect( new Rect( rect.width * 0.5f, TitleHeight, rect.width * 0.5f - 1, rect.height - TitleHeight - 4 ), 4 );
		}
		else
		{
			//	Paint.DrawRect( new Rect( 0, TitleHeight, rect.width - 1, rect.height - TitleHeight ), 2 );
		}

		if ( Paint.HasSelected )
		{
			Paint.SetPen( SelectionOutline, 2.0f );
			Paint.SetBrushEmpty();
			Paint.DrawRect( rect.Contract( 1 ), 4.0f );
		}
		else if ( Paint.HasMouseOver )
		{
			Paint.SetPen( SelectionOutline.WithAlpha( 0.4f ), 2.0f );
			Paint.SetBrushEmpty();
			Paint.DrawRect( rect.Contract( 1 ), 4.0f );
		}

		// titlebar
		{
			rect = new Rect( rect.Position, new Vector2( rect.width, TitleHeight ) ).Contract( 3 );

			// title background
			Paint.ClearPen();
			Paint.SetBrush( PrimaryColor.WithAlpha( 0.2f ) );
			Paint.DrawRect( rect, 3 );

			if ( DisplayInfo.Icon != null )
			{
				Paint.SetPen( PrimaryColor.WithAlpha( 0.7f ) );
				Paint.DrawMaterialIcon( rect.Contract( 4 ), DisplayInfo.Icon, 17, TextFlag.LeftCenter );
				rect.left += 18;
			}

			var title = Node.GetTitle() ?? DisplayInfo.Name;

			Paint.SetDefaultFont( 7, 500 );
			Paint.SetPen( PrimaryColor );
			Paint.DrawText( rect.Contract( 5, 0 ), title, TextFlag.LeftCenter );
		}
	}

	protected override void OnMouseReleased( GraphicsMouseEvent e )
	{
		base.OnMouseReleased( e );


	}

	internal void DraggingOutput( PlugOut nodeOutput, Vector2 scenePosition, Connection source = null )
	{
		Graph?.DraggingOutput( this, nodeOutput, scenePosition, source );
	}

	internal void DroppedOutput( PlugOut nodeOutput, Vector2 scenePosition, Connection source = null )
	{
		Graph?.DroppedOutput( this, nodeOutput, scenePosition, source );
	}

	protected override void OnPositionChanged()
	{
		Position = Position.SnapToGrid( 16.0f );

		Graph?.NodePositionChanged( this );
	}
}
