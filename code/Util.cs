using Tools;

namespace SandMix.Tool;

internal static class Util
{
	public static Pixmap RenderIcon( string name )
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

		return icon;
	}
}
