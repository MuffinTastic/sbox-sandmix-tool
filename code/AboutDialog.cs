using System.Diagnostics;
using Tools;

namespace SandMixTool;

public class AboutDialog : Dialog
{
	private const string TitleImageFile = "assets/about-icon.png";

	private const string Version = "0.0.1";
	private const string RepoURL = "https://www.example.com/";
	private static readonly string[] Authors = { "MuffinTastic" };

	public AboutDialog( Widget parent = null ) : base( parent )
	{
		Window.Size = new Vector2( 350, 250 );
		Window.CloseButtonVisible = true;
		Window.MaximumSize = Size;
		Window.Title = "About s&mix";
		Window.SetModal( true );

		CreateUI();
	}

	private void CreateUI()
	{
		SetLayout( LayoutMode.TopToBottom );
		Layout.Margin = 10;
		Layout.Spacing = 10;

		Layout.Add( new TitleImage( TitleImageFile, this ) );

		var infoArea = new ScrollArea( this );
		infoArea.Canvas = new Widget( infoArea );
		infoArea.Canvas.SetStyles( "background-color: transparent; " );
		infoArea.Canvas.SetLayout( LayoutMode.TopToBottom );
		var l = infoArea.Canvas.Layout;
		l.Spacing = 5;

		{
			var titleVersionLabel = new Label( infoArea.Canvas );
			titleVersionLabel.Text = $"s&mix <b>v{Version}</b>";
			titleVersionLabel.Alignment = TextFlag.HCenter;
			l.Add( titleVersionLabel );;

			var authorsLabel = new Label( infoArea.Canvas );
			authorsLabel.Text = "Authors:\n";
			foreach ( var author in Authors )
			{
				authorsLabel.Text += $"{author}\n";
			}
			authorsLabel.Alignment = TextFlag.HCenter;
			l.Add( authorsLabel );

			l.AddStretchCell();
		}

		Layout.Add( infoArea );

		var repoButton = new Button( this );
		repoButton.Text = "Visit repo";
		repoButton.Clicked += () => Process.Start( "explorer", RepoURL );
		Layout.Add( repoButton );
	}
}

/// <summary>
/// Centered image widget
/// This is a workaround for not being able to set a Label's pixmap
/// </summary>
public class TitleImage : Widget
{
	private Pixmap image;
	private Vector2 imageSize;

	public TitleImage( string imageFile, Widget parent = null ) : base( parent )
	{
		image = Pixmap.FromFile( imageFile );
		imageSize = new Vector2( image.Width, image.Height + 10 ); // hacky vertical padding
		MinimumSize = imageSize;
	}

	protected override void OnPaint()
	{
		var imageCenter = Size / 2;
		Paint.Draw( new Rect( imageCenter - imageSize / 2, imageSize ), image );
	}
}
