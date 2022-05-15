using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Tools;

namespace SandMixTool.NodeGraph;

public partial class Selection : Tools.GraphicsItem
{
	public static readonly Vector2 DrawPadding = new Vector2( 50.0f );
	public Color PrimaryColor = Color.Parse( "#93b4d6" ) ?? default;
	public Vector2 DrawSize { get; set; }

	Vector2 lastDrawSize;

	public GraphView Graph;

	public Selection( GraphView graph )
	{
		Graph = graph;
	}

	protected override void OnPaint()
	{
		lastDrawSize = DrawSize;
		var rect = new Rect( DrawPadding, DrawSize - DrawPadding * 2 );

		Paint.SetPen( PrimaryColor.WithAlpha( 0.8f ), 1.0f );
		Paint.SetBrush( PrimaryColor.Darken( 0.5f ).WithAlpha( 0.3f ) );
		Paint.DrawRect( rect, 5.0f );
	}

	public void UpdateSelection( Vector2 start, Vector2 end )
	{
		var left = MathF.Min( start.x, end.x );
		var right = MathF.Max( start.x, end.x );
		var top = MathF.Min( start.y, end.y );
		var bottom = MathF.Max( start.y, end.y );

		var position = new Vector2( left, top ) - DrawPadding;
		var size = new Vector2( right - left, bottom - top ) + DrawPadding * 2;

		Position = position;

		// this is kinda dumb, but it's the only way to avoid leaving a weird trail when you move it quickly
		if ( start.x < end.x || start.y < end.y ) // bottom right quad
		{
			Size = lastDrawSize;
			Log.Info( $"{size - DrawSize}" );
			DrawSize = size;
		}
		else
		{
			Size = DrawSize = size;
		}

		Update();

		var selectionRect = new Rect( position + DrawPadding, size - DrawPadding * 2 );

		var items = Graph.Items.OfType<NodeUI>();
		foreach ( var item in items )
		{
			item.Selected = selectionRect.IsInside( item.Rect );
		}
	}
}
