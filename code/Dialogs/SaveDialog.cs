using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools;

namespace SandMix.Tool.Dialogs;

public class SaveDialog : Dialog
{
	public enum Result
	{
		Yes,
		No,
		Cancel
	}

	private bool AlreadyTriggered = false;
	private event Action<Result> Triggered;

	private SaveDialog( Widget parent = null ) : base( parent )
	{
		Window.Size = new Vector2( 400, 120 );
		Window.IsDialog = true;
		Window.MaximumSize = Size;
		Window.Title = Util.GetLocalized( "#smix.ui.save" );
		Window.SetModal( true );

		Window.SetWindowIcon( Util.RenderIcon( "save" ) );

		Triggered += OnTriggered;

		CreateUI();
		Show();
	}

	private void CreateUI()
	{
		SetLayout( LayoutMode.TopToBottom );
		Layout.Margin = 10;
		Layout.Spacing = 10;

		var messageLabel = new Label( this );
		messageLabel.Text = Util.GetLocalized( "#smix.ui.save.message" );
		Layout.Add( messageLabel );

		var hl = Layout.Add( LayoutMode.LeftToRight );
		hl.Spacing = 4;
		{
			var yesButton = new Button( this );
			yesButton.Text = Util.GetLocalized( "#smix.ui.save.yes" );
			yesButton.Clicked += () => Triggered( Result.Yes );
			hl.Add( yesButton, 1 );

			var noButton = new Button( this );
			noButton.Text = Util.GetLocalized( "#smix.ui.save.no" );
			noButton.Clicked += () => Triggered( Result.No );
			hl.Add( noButton, 1 );

			var cancelButton = new Button( this );
			cancelButton.Text = Util.GetLocalized( "#smix.ui.save.cancel" );
			cancelButton.Clicked += () => Triggered( Result.Cancel );
			hl.Add( cancelButton, 1 );
		}
	}

	private void OnTriggered( Result result )
	{
		Close();
	}

	public static Task<Result> RunAsync( Widget parent = null )
	{
		TaskCompletionSource<Result> tcs = new TaskCompletionSource<Result>();
		var dialog = new SaveDialog( parent );
		dialog.Triggered += result => tcs.SetResult( result );
		return tcs.Task;
	}
}
