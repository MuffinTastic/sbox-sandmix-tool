using Sandbox;
using System.Text.Json;
using Tools;

namespace SandMixTool;

[Tool( "s&mix", "surround_sound", "Dynamic node-based sound mixer!" )]
public class MainWindow : Window
{
	/*
	[Menu( "Editor", "Help/About", "info" )]
	public static void OpenWindow()
	{
		Log.Info( "Open Window!" );
	}

	[Menu( "Hammer", "Sbox/About", "info" )]
	public static void OpenWindow2()
	{
		new SandMixTool();
	}
	*/

	public MainWindow()
	{
		Title = "s&mix";
		Size = new Vector2( 1920, 1080 );

		CreateUI();
		Show();
	}

	public void BuildMenu()
	{
		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "Open" );
		file.AddOption( "Save" );
		file.AddOption( "Quit" ).Triggered += () => Close();

		var help = MenuBar.AddMenu( "Help" );
		help.AddOption( "Documentation" );
		help.AddSeparator();
		help.AddOption( "About" ).Triggered += () => new AboutDialog( this ).Show();
	}

	public Curve2D Curve { get; set; }

	public void CreateUI()
	{
		Clear();

		BuildMenu();

		var c = new Curve2D();
		c.AddPoint( 0, 0 );
		c.AddPoint( 0.2f, 20 );
		c.AddPoint( 0.5f, 60 );
		c.AddPoint( 0.8f, 10 );
		Curve = c;

		var w = new Widget( null );
		w.SetLayout( LayoutMode.LeftToRight );
		w.Layout.Margin = 32;
		w.Layout.Spacing = 8;

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

		Canvas = w;
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
