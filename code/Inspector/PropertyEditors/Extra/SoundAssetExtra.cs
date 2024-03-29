﻿using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( "assetextra:vsnd" )]
public class SoundAssetExtra : Widget
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

	Button FocusButton;
	Button PlayButton;

	public SoundAssetExtra( Widget parent ) : base( parent )
	{
		Cursor = CursorShape.Finger;
		SetLayout( LayoutMode.RightToLeft );
		SetSizeMode( SizeMode.CanShrink, SizeMode.CanShrink );
		MouseTracking = true;
		AcceptDrops = true;

		PlayButton ??= new Button( "", "volume_up", this );
		PlayButton.ButtonType = "primary";
		PlayButton.ToolTip = "Play track";
		PlayButton.Clicked = () => Utility.PlayAssetSound( Asset );
		PlayButton.MaximumSize = Theme.RowHeight;
		Layout.Add( PlayButton );

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
		FocusButton.Visible = Asset != null;
		PlayButton.Visible = Asset != null;
		AdjustSize();
	}
}
