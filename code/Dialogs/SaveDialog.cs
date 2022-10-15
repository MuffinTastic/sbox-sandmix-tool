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
		Window.Size = new Vector2( 400, 150 );
		Window.IsDialog = true;
		Window.MaximumSize = Size;
		Window.Title = "Save changes?";
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

		var warningLabel1 = new Label( this );
		warningLabel1.Text = $"There are unsaved changes!";
		Layout.Add( warningLabel1 );

		var warningLabel2 = new Label( this );
		warningLabel2.Text = "Do you wish to save them?";
		Layout.Add( warningLabel2 );

		var hl = Layout.Add( LayoutMode.LeftToRight );
		hl.Spacing = 4;
		{
			var yesButton = new Button( this );
			yesButton.Text = "Yes";
			yesButton.Clicked += () => Triggered( Result.Yes );
			hl.Add( yesButton, 1 );

			var noButton = new Button( this );
			noButton.Text = "No";
			noButton.Clicked += () => Triggered( Result.No );
			hl.Add( noButton, 1 );

			var cancelButton = new Button( this );
			cancelButton.Text = "Cancel";
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
