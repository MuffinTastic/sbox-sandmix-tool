using System.Text;
using System.Threading.Tasks;
using Tools;
using SandMix.Nodes;
using SandMix.Tool.Widgets;
using Sandbox;

namespace SandMix.Tool;

[CanEdit( $"asset:{MixGraphResource.FileExtension}" )]
[CanEdit( $"asset:{EffectGraphResource.FileExtension}" )]
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

		var file = MenuBar.AddMenu( Util.GetLocalized( "#smix.ui.menubar.file" ) );
		{
			var newMixGraph = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.newmixgraph" ) );
			newMixGraph.Triggered += () => _ = File.FileNew( GraphType.Mix );
			newMixGraph.Shortcut = "Ctrl+N";

			var newEffectGraph = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.neweffectgraph" ) );
			newEffectGraph.Triggered += () => _ = File.FileNew( GraphType.Effect );
			newEffectGraph.Shortcut = "Ctrl+Shift+N";

			var open = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.open" ) );
			open.Triggered += () => _ = File.FileOpen();
			open.Shortcut = "Ctrl+O";

			file.AddSeparator();

			MenuOptions.FileCloseOption = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.close" ) );
			MenuOptions.FileCloseOption.Triggered += () => _ = File.FileClose();

			file.AddSeparator();

			MenuOptions.FileSaveOption = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.save" ) );
			MenuOptions.FileSaveOption.Triggered += () => File.Save();
			MenuOptions.FileSaveOption.Shortcut = "Ctrl+S";

			MenuOptions.FileSaveAsOption = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.saveas" ) );
			MenuOptions.FileSaveAsOption.Triggered += () => File.SaveAs();

			file.AddSeparator();

			var quit = file.AddOption( Util.GetLocalized( "#smix.ui.menubar.file.quit" ) );
			quit.Triggered += () => Close();
		}

		var edit = MenuBar.AddMenu( Util.GetLocalized( "#smix.ui.menubar.edit" ) );
		{
			MenuOptions.EditUndoOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.undo" ) );
			MenuOptions.EditUndoOption.Triggered += () => File.EditUndo();
			MenuOptions.EditUndoOption.Shortcut = "Ctrl+Z";

			MenuOptions.EditRedoOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.redo" ) );
			MenuOptions.EditRedoOption.Triggered += () => File.EditRedo();
			MenuOptions.EditRedoOption.Shortcut = "Ctrl+Y";

			edit.AddSeparator();

			MenuOptions.EditCutOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.cut" ) );
			MenuOptions.EditCutOption.Triggered += () => File.EditCut();
			MenuOptions.EditCutOption.Shortcut = "Ctrl+X";

			MenuOptions.EditCopyOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.copy" ) );
			MenuOptions.EditCopyOption.Triggered += () => File.EditCopy();
			MenuOptions.EditCopyOption.Shortcut = "Ctrl+C";

			MenuOptions.EditPasteOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.paste" ) );
			MenuOptions.EditPasteOption.Triggered += () => File.EditPaste();
			MenuOptions.EditPasteOption.Shortcut = "Ctrl+V";

			MenuOptions.EditDeleteOption = edit.AddOption( Util.GetLocalized( "#smix.ui.menubar.edit.delete" ) );
			MenuOptions.EditDeleteOption.Triggered += () => File.EditDelete();
			MenuOptions.EditDeleteOption.Shortcut = "Del";
		}

		var view = MenuBar.AddMenu( Util.GetLocalized( "#smix.ui.menubar.view" ) );
		{
			view.AddOption( File.GetToggleViewOption() );

			MenuOptions.ViewPreviewOption = Preview.GetToggleViewOption();
			view.AddOption( MenuOptions.ViewPreviewOption );

			view.AddOption( Inspector.GetToggleViewOption() );

			view.AddSeparator();

			MenuOptions.ViewRecenterGraphViewOption = view.AddOption( Util.GetLocalized( "#smix.ui.menubar.view.recentergraphview" ) );
			MenuOptions.ViewRecenterGraphViewOption.Triggered += () => File.RecenterGraphView();
		}

		var help = MenuBar.AddMenu( Util.GetLocalized( "#smix.ui.menubar.help" ) );
		{
			var documentation = help.AddOption( Util.GetLocalized( "#smix.ui.menubar.help.documentation" ) );
			
			help.AddSeparator();

			var about = help.AddOption( Util.GetLocalized( "#smix.ui.menubar.help.about", new() { ["ProjectName"] = SandMix.ProjectName } ) );
			about.Triggered += () => new Dialogs.AboutDialog( this ).Show();
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
		SetWindowIcon( Util.RenderIcon( name ) );
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
