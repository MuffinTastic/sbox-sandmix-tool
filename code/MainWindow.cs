using Sandbox;
using SandMixTool.Widgets;
using System.Text.Json;
using Tools;

namespace SandMixTool;

[Tool( SandMixTool.ProjectName, "surround_sound", SandMixTool.ProjectTagline )]
public class MainWindow : Window
{
	public static MainWindow Instance { get; set; }

	private MixGraphWidget MixGraph;

	public MainWindow()
	{
		Log.Info( Instance );
		
		if ( Instance is not null )
		{
			Destroy();
			Instance.Focus();
		}

		Title = SandMixTool.ProjectName;
		Size = new Vector2( 1920, 1080 );

		CreateUI();
		Show();
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		Instance = null;
	}

	public void BuildMenu()
	{
		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "Open" );
		file.AddOption( "Save" ).Triggered += () => MixGraph?.SaveGraph();
		file.AddOption( "Quit" ).Triggered += () => Close();

		var view = MenuBar.AddMenu( "View" );

		var help = MenuBar.AddMenu( "Help" );
		help.AddOption( "Documentation" );
		help.AddSeparator();
		help.AddOption( "About" ).Triggered += () => new AboutDialog( this ).Show();
	}

	public void CreateUI()
	{
		Clear();

		BuildMenu();

		MixGraph = new MixGraphWidget( this );
		Dock( MixGraph, DockArea.Left );

		var p = new PreviewWidget( this );
		Dock( p, DockArea.Right );

		var i = new InspectorWidget( MixGraph.GraphView, this );
		Dock( i, DockArea.Right );

		/*{
			var e = new Curve2DEditor( this );
			e.Bind( "Value" ).From( this, x => x.Curve );
			e.ValueRange = new Vector2( 100, 0 );
			w.Layout.Add( e );
		}

		var right = w.Layout.Add( LayoutMode.TopToBottom );
		right.Spacing = 8;
		{
			var e = new Curve2DEditor( this );
			e.Bind( "Value" ).From( this, x => x.Curve );
			e.ValueRange = new Vector2( 100, 0 );
			right.Add( e );
		}

		{
			var e = new Curve2DEditor( this );
			e.Bind( "Value" ).From( this, x => x.Curve );
			e.ValueRange = new Vector2( 100, 0 );
			right.Add( e );
		}
		*/
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		//Paint.DrawText( 0, JsonSerializer.Serialize( Curve ) );
	}

	[Sandbox.Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}
}
