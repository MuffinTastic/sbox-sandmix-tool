﻿using Sandbox;
using SandMix.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tools;

namespace SandMix.Tool.NodeGraph;

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

		UpdateTooltip();

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

	public void UpdateLanguage()
	{
		if ( !IsValid )
		{
			return;
		}

		foreach ( var plug in Inputs )
		{
			plug.UpdateLanguage();
		}

		foreach ( var plug in Outputs )
		{
			plug.UpdateLanguage();
		}

		Update();
	}

	public void UpdateTooltip()
	{
		DisplayInfo = Sandbox.DisplayInfo.For( Node );

		var tooltipBuilder = new StringBuilder();

		tooltipBuilder.Append( Util.GetLocalized( DisplayInfo.Name ) );
		tooltipBuilder.Append( '\n' );
		tooltipBuilder.Append( Util.GetLocalized( DisplayInfo.Description ?? "#smix.node.nodescription" ) );

		if ( !string.IsNullOrEmpty( Node.Name ) )
		{
			tooltipBuilder.Append( '\n' );
			tooltipBuilder.Append( Util.GetLocalized( "#smix.node.name" ) );
			tooltipBuilder.Append( ": " );
			tooltipBuilder.Append( Node.Name );
		}

		if ( !string.IsNullOrEmpty( Node.Comment ) )
		{
			tooltipBuilder.Append( '\n' );

			var commentBuilder = new StringBuilder();
			commentBuilder.Append( Util.GetLocalized( "#smix.node.comment" ) );
			commentBuilder.Append( ": " );

			var curLen = commentBuilder.Length;
			const int rows = 64;

			foreach ( var word in Node.Comment.Split(' ') )
			{
				curLen += word.Length + 1; // + space
			
				if ( curLen >= rows )
				{
					commentBuilder.Append( '\n' );
					curLen = 0;
				}

				commentBuilder.Append( word );
				commentBuilder.Append(' ');
			}

			tooltipBuilder.Append( commentBuilder );
		}

		Tooltip = tooltipBuilder.ToString().Trim();
	}

	void Layout()
	{
		var inputHeight = Inputs.Sum( x => x.Size.y );
		var outputHeight = Outputs.Sum( x => x.Size.y );

		var top = TitleHeight;
		var columnWidth = 160.0f;
		var width = columnWidth * 1.6f;

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
		UpdateTooltip(); // this kinda sux but it's the only reliable way to do it

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
			Paint.DrawRect( rect.Shrink( 1 ), 4.0f );
		}
		else if ( Paint.HasMouseOver )
		{
			Paint.SetPen( SelectionOutline.WithAlpha( 0.4f ), 2.0f );
			Paint.SetBrushEmpty();
			Paint.DrawRect( rect.Shrink( 1 ), 4.0f );
		}

		// titlebar
		{
			var iconSize = 17;
			rect = new Rect( rect.Position, new Vector2( rect.Width, TitleHeight ) ).Shrink( 3 );

			// title background
			Paint.ClearPen();
			Paint.SetBrush( PrimaryColor.WithAlpha( 0.2f ) );
			Paint.DrawRect( rect, 3 );

			if ( DisplayInfo.Icon != null )
			{
				Paint.SetPen( PrimaryColor.WithAlpha( 0.7f ) );
				Paint.DrawIcon( rect.Shrink( 4 ), DisplayInfo.Icon, iconSize, TextFlag.LeftCenter );
				rect.Left += 18;
			}

			var title = Util.GetLocalized( DisplayInfo.Name );
			if ( !String.IsNullOrEmpty( Node.Name ) )
				title += $" - {Node.Name}";

			var hasComment = !string.IsNullOrEmpty( Node.Comment );

			Paint.SetDefaultFont( 7, 500 );
			Paint.SetPen( PrimaryColor );
			var textRect = rect.Shrink( 5 + (hasComment ? 7 : 0), 0 );

			if ( Paint.MeasureText(textRect, title, TextFlag.LeftCenter).Width > textRect.Width )
			{
				string croppedTitle = title + "...";

				while ( Paint.MeasureText( textRect, croppedTitle, TextFlag.LeftCenter ).Width > textRect.Width )
				{
					title = title.Substring( 0, title.Length - 1 );
					croppedTitle = title + "...";
				}

				title = croppedTitle;
			}

			Paint.DrawText( rect.Shrink( 5, 0 ), title, TextFlag.LeftCenter );

			if ( hasComment )
			{
				Paint.SetPen( PrimaryColor.WithAlpha( 0.7f ) );
				Paint.DrawIcon( rect.Shrink( 4 ), "sticky_note_2", iconSize, TextFlag.RightCenter );
			}
		}
	}

	internal void DraggingOutput( PlugOut nodeOutput, Vector2 scenePosition, ConnectionUI source = null )
	{
		Graph?.DraggingOutput( this, nodeOutput, scenePosition, source );
	}

	internal void DroppedOutput( PlugOut nodeOutput, Vector2 scenePosition, ConnectionUI source = null )
	{
		Graph?.DroppedOutput( this, nodeOutput, scenePosition, source );
	}

	protected override void OnPositionChanged()
	{
		Position = Position.SnapToGrid( 16.0f );
		Node.Position = Position;

		Graph?.NodePositionChanged( this );
	}
}
