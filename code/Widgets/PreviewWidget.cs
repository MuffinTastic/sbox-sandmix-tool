using System;
using Tools;
using SandMix.Tool.NodeGraph;

namespace SandMix.Tool.Widgets;

public class PreviewWidget : DockWidget
{
	public PreviewWidget( GraphView graphView, Widget parent = null ) : base( "Preview", "preview", parent )
	{
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
