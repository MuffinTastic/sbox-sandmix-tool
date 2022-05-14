using Sandbox;
using Tools;

public static class Theme
{
	public static Color White;
	public static Color Grey;
	public static Color Black;

	public static Color Red;
	public static Color Green;
	public static Color Blue;
	public static Color Yellow;

	public static Color WidgetBackground;
	public static Color ControlBackground;
	public static Color ControlText;
	public static Color Primary;

	public static Color ButtonDefault;

	public static float RowHeight;
	public static float ControlRadius;

	static Theme()
	{
		Init();
	}

	[Event.Hotload]
	public static void Init()
	{
		Blue = Color.Parse( "#77BBFF" ) ?? default;
		Green = Color.Parse( "#B0E24D" ) ?? default;
		Red = Color.Parse( "#FB5A5A" ) ?? default;
		Yellow = Color.Parse( "#E6DB74" ) ?? default;

		White = Color.Parse( "#F8F8F8" ) ?? default;
		Grey = Color.Parse( "#808080" ) ?? default;
		Black = Color.Parse( "#111111" ) ?? default;

		Primary = Color.Parse( "#3472E6" ) ?? default;

		WidgetBackground = Color.Parse( "#38393c" ) ?? default;
		ControlBackground = Color.Parse( "#201F21" ) ?? default;
		ButtonDefault = Color.Parse( "#78797c" ) ?? default;
		ControlText = Color.Parse( "#ccc" ) ?? default;
		ControlRadius = 3.0f;

		RowHeight = 22;
	}

	internal static void DrawFilename( Rect rect, string filename, TextFlag flags, Color color )
	{
		var dir = System.IO.Path.GetDirectoryName( filename ) + "/";
		var file = System.IO.Path.GetFileNameWithoutExtension( filename );
		var extension = System.IO.Path.GetExtension( filename );

		// if we really cared we could do this better
		var size = Paint.MeasureText( rect, filename, flags );
		var overshoot = (size.width - rect.width) + 5;
		if ( overshoot > 0 )
		{
			overshoot += 10;
			dir = ".." + dir.Substring( (overshoot / 4).CeilToInt() );
		}

		dir = dir.Replace( '\\', '/' );

		Paint.SetPen( color.Darken( 0.3f ) );
		var r = Paint.DrawText( rect, dir, flags );
		rect.left += r.width;
		Paint.SetPen( color );
		r = Paint.DrawText( rect, file, flags );
		rect.left += r.width;
		Paint.SetPen( color.Darken( 0.1f ) );
		r = Paint.DrawText( rect, extension, flags );
	}

	internal static void DrawButton( Rect rect, Color? color = default )
	{
		var c = color ?? ButtonDefault;
		float radius = 1.5f;
		Paint.Antialiasing = true;
		Paint.SetPenEmpty();

		if ( Paint.HasMouseOver )
			c = c.Lighten( 0.2f );

		rect = rect.Contract( 1 );
		rect.Position -= 1;

		Paint.SetBrush( Color.White.WithAlpha( 0.3f ) );
		Paint.DrawRect( rect, radius );

		rect.Position += 2;

		Paint.SetBrush( Color.Black.WithAlpha( 0.3f ) );
		Paint.DrawRect( rect, radius );

		rect.Position -= 1;

		Paint.SetBrushLinear( rect.TopLeft, rect.BottomLeft + Vector2.Left * 2, c.Darken( 0.2f ), c.Darken( 0.3f ) );
		Paint.DrawRect( rect, radius );

		Paint.SetPen( c.Lighten( 0.5f ) );
	}
}
