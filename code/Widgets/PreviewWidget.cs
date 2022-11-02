using System;
using Sandbox;
using Tools;
using SandMix.Tool.NodeGraph;
using SandMix.Tool.Preview;

namespace SandMix.Tool.Widgets;

public class PreviewWidget : DockWidget
{
	public PreviewWidget( GraphView graphView, Widget parent = null ) : base( null, "preview", parent )
	{
		Title = Util.GetLocalized( "#smix.ui.preview" );

		Widget = new Widget( this );

		Widget.SetLayout( LayoutMode.TopToBottom );
		Log.Info( $"{Widget.Width} {Widget.Height}" );

		Widget.Layout.AddStretchCell();
		Widget.Layout.Add( new AudioMonitor( Widget ) );
	}

	public void UpdateLanguage()
	{
		if ( !IsValid )
		{
			return;
		}

		Title = Util.GetLocalized( "#smix.ui.preview" );
	}

	public void SetGraphView( GraphView graphView )
	{

	}

	public void UnsetGraphView()
	{
		
	}
}
