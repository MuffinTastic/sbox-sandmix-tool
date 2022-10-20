using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( $"assetextra:{EffectGraphResource.FileExtension}" )]
public class EffectExtra : Widget
{
	Asset asset;
	public Asset Asset
	{
		get => asset;
		set
		{
			asset = value;
			OnAssetChanged();
		}
	}

	Button OpenInNewButton;
	Button FocusButton;

	public EffectExtra( Widget parent ) : base( parent )
	{
		Cursor = CursorShape.Finger;
		SetLayout( LayoutMode.RightToLeft );
		SetSizeMode( SizeMode.CanShrink, SizeMode.CanShrink );
		MouseTracking = true;
		AcceptDrops = true;

		OpenInNewButton ??= new Button( "", "open_in_new", this );
		OpenInNewButton.ButtonType = "primary";
		OpenInNewButton.ToolTip = "Open effect in new window";
		OpenInNewButton.Clicked = () => ToolWindow.OpenNew( Asset );
		OpenInNewButton.MaximumSize = Theme.RowHeight;
		Layout.Add( OpenInNewButton );

		FocusButton ??= new Button( "", "filter_center_focus", this );
		FocusButton.ButtonType = "primary";
		FocusButton.ToolTip = "Focus on effect in s&box editor";
		FocusButton.Clicked = () => {
			MainAssetBrowser.Instance?.FocusOnAsset( Asset );
			Utility.InspectorObject = Asset;
			MainAssetBrowser.Instance?.Focus();
		};
		FocusButton.MaximumSize = Theme.RowHeight;
		Layout.Add( FocusButton );

		Layout.Margin = 4;
		Layout.Spacing = 4;

		OnAssetChanged();
	}

	void OnAssetChanged()
	{
		OpenInNewButton.Visible = Asset != null;
		FocusButton.Visible = Asset != null;
		AdjustSize();
	}
}
