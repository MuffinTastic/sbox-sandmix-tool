﻿using Tools;
using Sandbox;

public class SheetHeader : Widget
{
	PropertySheet Sheet { get; init; }

	public SheetHeader( PropertySheet parent ) : base( parent )
	{
		MinimumSize = 48;
		Sheet = parent;
		SetSizeMode( SizeMode.Default, SizeMode.CanShrink );
	}

	protected override void OnPaint()
	{
		var info = DisplayInfo.For( Sheet.Target );
		var rect = new Rect( 0, Size );

		Paint.SetPenEmpty();
		Paint.SetBrush( Theme.Black.Lighten( 0.9f ) );
		Paint.DrawRect( rect );

		rect.left += 8;


		Paint.SetPen( Theme.Green );

		var iconRect = Paint.DrawMaterialIcon( rect, info.Icon ?? "question_mark", 24, TextFlag.LeftCenter );

		rect.top += 8;
		rect.left = iconRect.right + 8;

		Paint.SetPen( Theme.Green );
		Paint.SetDefaultFont( 10, 500 );
		var titleRect = Paint.DrawText( rect, $"{Sheet.Target}", TextFlag.LeftTop );

		rect.top = titleRect.bottom + 2;

		Paint.SetPen( Theme.Green.WithAlpha( 0.6f ) );
		Paint.SetDefaultFont( 8, 400 );
		Paint.DrawText( rect, $"{info.Name}", TextFlag.LeftTop );
	}
}
