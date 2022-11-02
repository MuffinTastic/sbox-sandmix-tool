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
		public Menu File;
		public Option FileNewMixGraph;
		public Option FileNewEffectGraph;
		public Option FileOpen;
		public Option FileCloseOption;
		public Option FileSaveOption;
		public Option FileSaveAsOption;
		public Option FileQuit;

		public Menu Edit;
		public Option EditUndoOption;
		public Option EditRedoOption;
		public Option EditCutOption;
		public Option EditCopyOption;
		public Option EditPasteOption;
		public Option EditDeleteOption;

		public Menu View;
		public Option ViewPreviewOption;
		public Option ViewRecenterGraphViewOption;

		public Menu Help;
		public Option HelpDocumentation;
		public Option HelpAbout;

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

		MenuOptions.File = MenuBar.AddMenu( "" );
		{
			MenuOptions.FileNewMixGraph = MenuOptions.File.AddOption( "_newmixgraph" );
			MenuOptions.FileNewMixGraph.Triggered += () => _ = File.FileNew( GraphType.Mix );
			MenuOptions.FileNewMixGraph.Shortcut = "Ctrl+N";

			MenuOptions.FileNewEffectGraph = MenuOptions.File.AddOption( "_neweffectgraph" );
			MenuOptions.FileNewEffectGraph.Triggered += () => _ = File.FileNew( GraphType.Effect );
			MenuOptions.FileNewEffectGraph.Shortcut = "Ctrl+Shift+N";

			MenuOptions.FileOpen = MenuOptions.File.AddOption( "_open" );
			MenuOptions.FileOpen.Triggered += () => _ = File.FileOpen();
			MenuOptions.FileOpen.Shortcut = "Ctrl+O";

			MenuOptions.File.AddSeparator();

			MenuOptions.FileCloseOption = MenuOptions.File.AddOption( "_close" );
			MenuOptions.FileCloseOption.Triggered += () => _ = File.FileClose();

			MenuOptions.File.AddSeparator();

			MenuOptions.FileSaveOption = MenuOptions.File.AddOption( "_save" );
			MenuOptions.FileSaveOption.Triggered += () => File.Save();
			MenuOptions.FileSaveOption.Shortcut = "Ctrl+S";

			MenuOptions.FileSaveAsOption = MenuOptions.File.AddOption( "_saveas" );
			MenuOptions.FileSaveAsOption.Triggered += () => File.SaveAs();

			MenuOptions.File.AddSeparator();

			MenuOptions.FileQuit = MenuOptions.File.AddOption( "_quit" );
			MenuOptions.FileQuit.Triggered += () => Close();
		}

		MenuOptions.Edit = MenuBar.AddMenu( "_edit" );
		{
			MenuOptions.EditUndoOption = MenuOptions.Edit.AddOption( "_undo" );
			MenuOptions.EditUndoOption.Triggered += () => File.EditUndo();
			MenuOptions.EditUndoOption.Shortcut = "Ctrl+Z";

			MenuOptions.EditRedoOption = MenuOptions.Edit.AddOption( "_redo" );
			MenuOptions.EditRedoOption.Triggered += () => File.EditRedo();
			MenuOptions.EditRedoOption.Shortcut = "Ctrl+Y";

			MenuOptions.Edit.AddSeparator();

			MenuOptions.EditCutOption = MenuOptions.Edit.AddOption( "_cut" );
			MenuOptions.EditCutOption.Triggered += () => File.EditCut();
			MenuOptions.EditCutOption.Shortcut = "Ctrl+X";

			MenuOptions.EditCopyOption = MenuOptions.Edit.AddOption( "_copy" );
			MenuOptions.EditCopyOption.Triggered += () => File.EditCopy();
			MenuOptions.EditCopyOption.Shortcut = "Ctrl+C";

			MenuOptions.EditPasteOption = MenuOptions.Edit.AddOption( "_paste" );
			MenuOptions.EditPasteOption.Triggered += () => File.EditPaste();
			MenuOptions.EditPasteOption.Shortcut = "Ctrl+V";

			MenuOptions.EditDeleteOption = MenuOptions.Edit.AddOption( "_delete" );
			MenuOptions.EditDeleteOption.Triggered += () => File.EditDelete();
			MenuOptions.EditDeleteOption.Shortcut = "Del";
		}

		MenuOptions.View = MenuBar.AddMenu( "_view" );
		{
			MenuOptions.View.AddOption( File.GetToggleViewOption() );

			MenuOptions.ViewPreviewOption = Preview.GetToggleViewOption();
			MenuOptions.View.AddOption( MenuOptions.ViewPreviewOption );

			MenuOptions.View.AddOption( Inspector.GetToggleViewOption() );

			MenuOptions.View.AddSeparator();

			MenuOptions.ViewRecenterGraphViewOption = MenuOptions.View.AddOption( "_recentergraphview" );
			MenuOptions.ViewRecenterGraphViewOption.Triggered += () => File.RecenterGraphView();
		}

		MenuOptions.Help = MenuBar.AddMenu( "_help" );
		{
			MenuOptions.HelpDocumentation = MenuOptions.Help.AddOption( "_documentation" );

			MenuOptions.Help.AddSeparator();

			MenuOptions.HelpAbout = MenuOptions.Help.AddOption( "_about" );
			MenuOptions.HelpAbout.Triggered += () => new Dialogs.AboutDialog( this ).Show();
		}

		SetMenuText();
	}

	private void SetMenuText()
	{
		if ( MenuOptions.FileNewMixGraph == null ) return;
		MenuOptions.File.Title							= Util.GetLocalized( "#smix.ui.menubar.file" );
		MenuOptions.FileNewMixGraph.Text				= Util.GetLocalized( "#smix.ui.menubar.file.newmixgraph" );
		MenuOptions.FileNewEffectGraph.Text				= Util.GetLocalized( "#smix.ui.menubar.file.neweffectgraph" );
		MenuOptions.FileOpen.Text						= Util.GetLocalized( "#smix.ui.menubar.file.open" );
		MenuOptions.FileCloseOption.Text				= Util.GetLocalized( "#smix.ui.menubar.file.close" );
		MenuOptions.FileSaveOption.Text					= Util.GetLocalized( "#smix.ui.menubar.file.save" );
		MenuOptions.FileSaveAsOption.Text				= Util.GetLocalized( "#smix.ui.menubar.file.saveas" );
		MenuOptions.FileQuit.Text						= Util.GetLocalized( "#smix.ui.menubar.file.quit" );

		MenuOptions.Edit.Title							= Util.GetLocalized( "#smix.ui.menubar.edit" );
		MenuOptions.EditUndoOption.Text					= Util.GetLocalized( "#smix.ui.menubar.edit.undo" );
		MenuOptions.EditRedoOption.Text					= Util.GetLocalized( "#smix.ui.menubar.edit.redo" );
		MenuOptions.EditCutOption.Text					= Util.GetLocalized( "#smix.ui.menubar.edit.cut" );
		MenuOptions.EditCopyOption.Text					= Util.GetLocalized( "#smix.ui.menubar.edit.copy" );
		MenuOptions.EditPasteOption.Text				= Util.GetLocalized( "#smix.ui.menubar.edit.paste" );
		MenuOptions.EditDeleteOption.Text				= Util.GetLocalized( "#smix.ui.menubar.edit.delete" );

		MenuOptions.View.Title							= Util.GetLocalized( "#smix.ui.menubar.view" );
		MenuOptions.ViewRecenterGraphViewOption.Text	= Util.GetLocalized( "#smix.ui.menubar.view.recentergraphview" );

		MenuOptions.Help.Title							= Util.GetLocalized( "#smix.ui.menubar.help" );
		MenuOptions.HelpDocumentation.Text				= Util.GetLocalized( "#smix.ui.menubar.help.documentation" );
		MenuOptions.HelpAbout.Text						= Util.GetLocalized( "#smix.ui.menubar.help.about",
															new() { ["ProjectName"] = SandMix.ProjectName }
														);

		Update();
	}

	[Event( "language.changed" )]
	private void UpdateLanguage()
	{
		if ( !IsValid )
		{
			return;
		}

		SetMenuText();
		File?.UpdateLanguage();
		Preview?.UpdateLanguage();
		Inspector?.UpdateLanguage();
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
		File.MinimumSize = (Vector2) 300.0f;

		Dock( File, DockArea.Left );
		Dock( Inspector, DockArea.Left );
		Dock( Preview, DockArea.Right );

		Inspector.Height = Inspector.MinimumHeight;
		Preview.Width = Preview.MinimumHeight;

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
