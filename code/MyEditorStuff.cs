using Tools;

namespace SandMixTool;

public class feeffe
{
	//
	// An Editor menu will put the option in the main editor's menu
	//
	[Menu( "Editor", "My Editor Stuff/Example Menu Option" )]
	public static void ExampleMenuOption()
	{
		Log.Info( "Menu option selected! Well done!" );
	}

	//
	// A dock is one of those tabby floaty windows, like the console and the addon manager.
	//
	[Dock( "Editor", "My Example Dock", "snippet_folder" )]
	public class MyExampleDock : Widget
	{
		Color color;

		public MyExampleDock( Widget parent ) : base( parent )
		{
			// Layout top to bottom
			SetLayout( LayoutMode.TopToBottom );

			var button = new Button( "Change Color", "color_lens" );
			button.Clicked = () =>
			{
				color = Color.Random;
				Update();
			};

			// Fill the top
			Layout.AddStretchCell();

			// Add a new layout cell to the bottom
			var bottomRow = Layout.Add( LayoutMode.LeftToRight );
			bottomRow.Margin = 16;
			bottomRow.AddStretchCell();
			bottomRow.Add( button );
		}

		protected override void OnPaint()
		{
			base.OnPaint();

			Paint.ClearPen();
			Paint.SetBrush( color );
			Paint.DrawRect( LocalRect );
		}
	}
}
