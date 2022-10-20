using Sandbox;
using System.Text;
using Tools;

namespace SandMix.Tool.Dialogs;

public class AboutDialog : Dialog
{
	private const string TitleImageFile = "assets/about-icon.png";

	public AboutDialog( Widget parent = null ) : base( parent )
	{
		Window.Size = new Vector2( 350, 250 );
		Window.IsDialog = true;
		Window.MaximumSize = Size;
		Window.Title = $"About {SandMix.ProjectName}";
		Window.Title = Util.GetLocalized( "#smix.ui.about", new() { ["ProjectName"] = SandMix.ProjectName } );
		Window.SetModal( true );

		Window.SetWindowIcon( Util.RenderIcon( "info" ) );

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
		infoArea.Canvas.SetStyles( "background-color: transparent;" );
		infoArea.Canvas.SetLayout( LayoutMode.TopToBottom );
		var l = infoArea.Canvas.Layout;
		l.Spacing = 5;

		{
			var titleVersionLabel = new Label( infoArea.Canvas );
			titleVersionLabel.Text = $"{SandMix.ProjectName} <b>v{SandMix.ProjectVersion}</b>";
			titleVersionLabel.Alignment = TextFlag.CenterHorizontally;
			l.Add( titleVersionLabel );

			var sb = new StringBuilder();
			sb.Append( Util.GetLocalized( "#smix.ui.about.authors" ) );
			sb.Append( ":\n" );

			foreach ( var author in SandMix.Authors )
			{
				sb.Append( author );
				sb.Append( "\n" );
			}

			var authorsLabel = new Label( infoArea.Canvas );
			authorsLabel.Text = sb.ToString();
			authorsLabel.Alignment = TextFlag.CenterHorizontally;
			l.Add( authorsLabel );

			l.AddStretchCell();
		}

		Layout.Add( infoArea );

		var repoButton = new Button( this );
		repoButton.Text = Util.GetLocalized( "#smix.ui.about.visitrepo" );
		repoButton.Clicked += () => Utility.OpenFolder( SandMix.ProjectRepoURL );
		repoButton.ToolTip = SandMix.ProjectRepoURL;
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
	private static readonly Vector2 Padding = new Vector2( 0, 10 ); // hack

	public TitleImage( string imageFile, Widget parent = null ) : base( parent )
	{
		image = Pixmap.FromFile( imageFile );
		imageSize = new Vector2( image.Width, image.Height );
		MinimumSize = imageSize + Padding;
	}

	protected override void OnPaint()
	{
		var imageCenter = Size / 2;
		Paint.Draw( new Rect( imageCenter - imageSize / 2, imageSize ), image );
	}
}
