using System.Collections.Generic;
using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( null, "enum" )]
public class EnumProperty<T> : ComboBox
{
	Dictionary<string, T> NameToEnum = new();
	Dictionary<T, DisplayInfo> EnumToDisplayInfo = new();

	public T Value
	{
		get
		{
			if ( NameToEnum.TryGetValue( CurrentText, out var val ) )
				return val;

			return default;
		}

		set
		{
			if ( EnumToDisplayInfo.TryGetValue( value, out var info ) )
			{
				TrySelectNamed( info.Name );
			}
		}
	}


	public EnumProperty( Widget parent ) : base( parent )
	{
		MinimumSize = Theme.RowHeight;
		Cursor = CursorShape.Finger;

		if ( typeof( T ).IsEnum )
		{
			var Values = typeof( T ).GetEnumValues();
			var Info = DisplayInfo.ForEnumValues( typeof( T ) );

			for ( int i = 0; i < Values.Length; i++ )
			{
				NameToEnum[Info[i].Name] = (T)Values.GetValue( i );
				EnumToDisplayInfo[(T)Values.GetValue( i )] = Info[i];

				AddItem( Info[i].Name, Info[i].Icon, description: Info[i].Description );
			}

			return;
		}
	}

	protected override void OnPaint()
	{
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.Lighten( IsUnderMouse ? 0.3f : 0.0f ) );
		Paint.DrawRect( LocalRect, Theme.ControlRadius );

		if ( NameToEnum.TryGetValue( CurrentText, out var value ) && EnumToDisplayInfo.TryGetValue( value, out var displayInfo ) )
		{
			var text = $"{CurrentText}";
			if ( text == "0" ) text = "(empty)";

			Paint.SetPen( Theme.ControlText );
			Paint.DrawIcon( LocalRect.Shrink( 8, 0 ), displayInfo.Icon ?? "check", 18, TextFlag.LeftCenter );
			Paint.SetDefaultFont();

			var r = LocalRect.Shrink( 8, 0 );
			r.Left += 25;
			Paint.DrawText( r, displayInfo.Name, TextFlag.LeftCenter );
		}
		else
		{
			var text = $"{CurrentText}";
			if ( text == "0" ) text = "(empty)";

			Paint.SetPen( Theme.ControlText );
			Paint.DrawText( LocalRect.Shrink( 8, 0 ), text, TextFlag.LeftCenter );
		}

		Paint.SetPen( Theme.ControlText.WithAlpha( 0.7f ) );
		Paint.DrawIcon( LocalRect, "Arrow_Drop_Down", 20, TextFlag.RightCenter );
	}
}
