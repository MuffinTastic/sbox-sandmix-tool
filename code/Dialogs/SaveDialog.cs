using SandMixTool.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tools;

namespace SandMixTool.Dialogs;

public class SaveDialog : Dialog
{
	public enum Result
	{
		Yes,
		No,
		Cancel
	}

	public event Action<Result> Triggered;

	public SaveDialog( IEnumerable<MixGraphWidget> mixGraphs, Widget parent = null ) : base( parent )
	{
		Window.Size = new Vector2( 400, 150 );
		Window.IsDialog = true;
		Window.MaximumSize = Size;
		Window.Title = "Save changes?";
		Window.SetModal( true );

		Triggered += OnTriggered;

		CreateUI( mixGraphs );
		Show();
	}

	private void CreateUI( IEnumerable<MixGraphWidget> mixGraphs )
	{
		SetLayout( LayoutMode.TopToBottom );
		Layout.Margin = 10;
		Layout.Spacing = 10;

		bool plural = mixGraphs.Count() > 1;
		var files = mixGraphs.Select( mg => mg.FilePath ?? mg.UnchangedTitle );

		var warningLabel1 = new Label( this );
		warningLabel1.Text = $"The following file{(plural ? "s" : "")} have unsaved changes:";
		Layout.Add( warningLabel1 );

		var filesLabel = new Label( this );
		filesLabel.Text = string.Join( '\n', files );
		Layout.Add( filesLabel );

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
			cancelButton.Clicked += () => Triggered( Result.Cancel);
			hl.Add( cancelButton, 1 );
		}
	}

	private void OnTriggered( Result result )
	{
		Close();
	}
}
