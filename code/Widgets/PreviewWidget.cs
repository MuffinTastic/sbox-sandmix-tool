using System;
using Sandbox;
using Tools;
using SandMix.Tool.NodeGraph;

namespace SandMix.Tool.Widgets;

public class PreviewWidget : DockWidget
{
	public PreviewWidget( GraphView graphView, Widget parent = null ) : base( null, "preview", parent )
	{
		Title = Util.GetLocalized( "#smix.ui.preview" );

		CreateUI();
	}

	private void CreateUI()
	{
		Widget = new Widget( this );
	}

	public void SetGraphView( GraphView graphView )
	{

	}

	public void UnsetGraphView()
	{
		
	}
}
