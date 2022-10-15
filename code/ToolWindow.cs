using System.Text;
using System.Threading.Tasks;
using Tools;
using SandMix.Nodes;
using SandMix.Tool.Widgets;

namespace SandMix.Tool;

[CanEdit( $"asset:{MixGraphResource.FileExtension}" )]
[CanEdit( $"asset:{EffectResource.FileExtension}" )]
[Tool( SandMix.ProjectName, SandMixTool.BaseIcon, SandMix.ProjectTagline )]
public class ToolWindow : Window, IAssetEditor
{
	public bool CanOpenMultipleAssets => false;

	private InspectorWidget Inspector;
	private PreviewWidget Preview;
	private FileWidget File;

	public struct MenuBarOptions
	{
		public Option FileCloseOption;
		public Option FileSaveOption;
		public Option FileSaveAsOption;

		public Option EditUndoOption;
		public Option EditRedoOption;
		public Option EditCutOption;
		public Option EditCopyOption;
		public Option EditPasteOption;
		public Option EditDeleteOption;

		public Option ViewPreviewOption;
		public Option ViewRecenterGraphViewOption;
	}

	private MenuBarOptions MenuOptions;

	public ToolWindow()
	{
		Hide();

		CreateUI();
	}

	public ToolWindow( Widget parent = null ) : base( parent )
	{
		// So it doesn't show in the main editor inspector
		Hide();
	}

	public void AssetOpen( Asset asset )
	{
		CreateUI();
		_ = File.FileOpen( asset );
	}

	public static void OpenNew( Asset asset )
	{
		var window = new ToolWindow();
		_ = window.File.FileOpen( asset );
	}

	public void BuildMenu()
	{
		MenuBar.Clear();

		MenuOptions = new MenuBarOptions();

		var file = MenuBar.AddMenu( "File" );
		{
			var newMix = file.AddOption( "New Mix" );
			newMix.Triggered += () => _ = File.FileNew( GraphType.Mix );
			newMix.Shortcut = "Ctrl+N";

			var newEffect = file.AddOption( "New Effect" );
			newEffect.Triggered += () => _ = File.FileNew( GraphType.Effect );
			newEffect.Shortcut = "Ctrl+Shift+N";

			var open = file.AddOption( "Open" );
			open.Triggered += () => _ = File.FileOpen();
			open.Shortcut = "Ctrl+O";

			file.AddSeparator();

			MenuOptions.FileCloseOption = file.AddOption( "Close" );
			MenuOptions.FileCloseOption.Triggered += () => _ = File.FileClose();

			file.AddSeparator();

			MenuOptions.FileSaveOption = new Option( title: "Save", icon: null, action: () => File.Save() );
			MenuOptions.FileSaveOption.Shortcut = "Ctrl+S";
			file.AddOption( MenuOptions.FileSaveOption );

			MenuOptions.FileSaveAsOption = new Option( title: "Save As", icon: null, action: () => File.SaveAs() );
			file.AddOption( MenuOptions.FileSaveAsOption );

			file.AddSeparator();

			file.AddOption( "Quit" ).Triggered += () => Close();
		}

		var edit = MenuBar.AddMenu( "Edit" );
		{
			MenuOptions.EditUndoOption = new Option( title: "Undo", icon: null, action: () => File.EditUndo() );
			MenuOptions.EditUndoOption.Shortcut = "Ctrl+Z";
			edit.AddOption( MenuOptions.EditUndoOption );

			MenuOptions.EditRedoOption = new Option( title: "Redo", icon: null, action: () => File.EditRedo() );
			MenuOptions.EditRedoOption.Shortcut = "Ctrl+Y";
			edit.AddOption( MenuOptions.EditRedoOption );

			edit.AddSeparator();

			MenuOptions.EditCutOption = new Option( title: "Cut", icon: null, action: () => File.EditCut() );
			MenuOptions.EditCutOption.Shortcut = "Ctrl+X";
			edit.AddOption( MenuOptions.EditCutOption );

			MenuOptions.EditCopyOption = new Option( title: "Copy", icon: null, action: () => File.EditCopy() );
			MenuOptions.EditCopyOption.Shortcut = "Ctrl+C";
			edit.AddOption( MenuOptions.EditCopyOption );

			MenuOptions.EditPasteOption = new Option( title: "Paste", icon: null, action: () => File.EditPaste() );
			MenuOptions.EditPasteOption.Shortcut = "Ctrl+V";
			edit.AddOption( MenuOptions.EditPasteOption );

			MenuOptions.EditDeleteOption = new Option( title: "Delete", icon: null, action: () => File.EditDelete() );
			MenuOptions.EditDeleteOption.Shortcut = "Del";
			edit.AddOption( MenuOptions.EditDeleteOption );
		}

		var view = MenuBar.AddMenu( "View" );
		{
			view.AddOption( File.GetToggleViewOption() );

			MenuOptions.ViewPreviewOption = Preview.GetToggleViewOption();
			view.AddOption( MenuOptions.ViewPreviewOption );

			view.AddOption( Inspector.GetToggleViewOption() );

			view.AddSeparator();

			MenuOptions.ViewRecenterGraphViewOption = new Option( title: "Recenter Graph View", icon: null, action: () => File.RecenterGraphView() );
			view.AddOption( MenuOptions.ViewRecenterGraphViewOption );
		}

		var help = MenuBar.AddMenu( "Help" );
		{
			help.AddOption( "Documentation" );
			help.AddSeparator();
			help.AddOption( "About" ).Triggered += () => new Dialogs.AboutDialog( this ).Show();
		}
	}

	public void CreateUI()
	{
		Title = SandMix.ProjectName;
		Size = new Vector2( 1920, 1080 );
		SetWindowIcon( SandMixTool.BaseIcon );

		Preview = new PreviewWidget( null, this );
		Preview.MinimumSize = (Vector2) 300.0f;

		Inspector = new InspectorWidget( this );
		Inspector.MinimumSize = (Vector2) 300.0f;

		File = new FileWidget( this, Preview, Inspector );
		File.MinimumSize = (Vector2)300.0f;

		Dock( File, DockArea.Left );
		Dock( Inspector, DockArea.Left );
		Dock( Preview, DockArea.Right );

		Inspector.Height = Inspector.MinimumHeight;

		BuildMenu();
		File.SetMenuBarOptions( MenuOptions );

		File.Show();
		Inspector.Show();
		Preview.Show();

		var showWindow = async () =>
		{
			await Task.Delay( 100 );

			if ( IsValid )
				Show();
		};

		showWindow();
	}

	public override void SetWindowIcon( string name )
	{
		var icon = new Pixmap( 128, 128 );
		icon.Clear( Color.Transparent );

		Paint.Target( icon );

		var r = new Rect( 0, 0, 128, 128 );

		Paint.ClearPen();

		Paint.SetBrush( Theme.Black );
		Paint.DrawRect( r, 16 );

		Paint.SetPen( Theme.White );
		Paint.DrawIcon( r, name, 128 );

		Paint.Target( null );

		base.SetWindowIcon( icon );
	}

	public void ResetWindowIcon()
	{
		SetWindowIcon( SandMixTool.BaseIcon );
	}

	public void SetWindowTitle( string title )
	{
		var sb = new StringBuilder();
		sb.Append( SandMix.ProjectName );

		if ( !string.IsNullOrEmpty( title ) )
		{
			sb.Append( " - " );
			sb.Append( title );
		}

		Title = sb.ToString();
	}

	public void ResetWindowTitle()
	{
		SetWindowTitle( null );
	}

	// FIXME: CloseEvent, this won't ever work properly until that parameter is added back
	protected override void OnClosed()
	{
		if ( File is null )
		{
			// Don't see why this should ever happen, but just in case
			// e.Accept();
			return;
		}

		async Task AsyncClose()
		{
			if ( !await File.FileClose() )
			{
				// e.Ignore();
			}

			// e.Accept();
		}

		_ = AsyncClose();
	}
}
