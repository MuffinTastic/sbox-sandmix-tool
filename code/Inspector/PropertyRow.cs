using Sandbox;
using System.Reflection;
using Tools;

namespace SandMixTool.Inspector;

public class PropertyRow : Widget
{
	DisplayInfo Info;

	int LabelWidth = 150;

	public PropertyRow( Widget parent ) : base( parent )
	{
		SetLayout( LayoutMode.LeftToRight );
		Layout.Margin = new( LabelWidth, 2, 8, 2 );
		MinimumSize = 23;
	}

	public void SetLabel( string text )
	{
		Info.Name = text;
	}

	public void SetLabel( PropertyInfo info )
	{
		Info = DisplayInfo.ForProperty( info );
	}

	public T SetWidget<T>( T w ) where T : Widget
	{
		Layout.Add( w, 1 );

		if ( Info.Placeholder != null && w is LineEdit e )
		{
			e.PlaceholderText = Info.Placeholder;
		}

		return w;
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		if ( string.IsNullOrEmpty( Info.Name ) )
			return;

		var size = LocalRect;
		size.width = LabelWidth - 16;

		if ( size.height > 28 )
			size.height = 28;

		size.left += 24;
		Paint.SetDefaultFont();
		Paint.SetPen( Theme.Grey.Lighten( 0.3f ) );
		Paint.DrawText( size, Info.Name, TextFlag.LeftCenter );

		//Paint.SetPen( Theme.Black.WithAlpha( 0.6f ) );
		//Paint.DrawLine( new Vector2( 0, LocalRect.bottom - 1 ), new Vector2( Size.x, LocalRect.bottom - 1 ) );
		//Paint.DrawLine( new Vector2( 0, LocalRect.top ), new Vector2( 0, LocalRect.bottom ) );

	}
}
