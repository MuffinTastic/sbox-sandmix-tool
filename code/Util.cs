using Sandbox;
using System.Collections.Generic;
using System.Text;
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

	public static string GetLocalized( string textToken )
	{
		if ( string.IsNullOrEmpty( textToken ) )
		{
			return null;
		}

		if ( !textToken.StartsWith('#') )
		{
			return textToken;
		}

		return Language.GetPhrase( textToken.Substring( 1 ) );
	}

	public static string GetLocalized( string textToken, Dictionary<string, object> data )
	{
		if ( string.IsNullOrEmpty( textToken ) )
		{
			return null;
		}

		if ( !textToken.StartsWith( '#' ) )
		{
			return textToken;
		}

		return Language.GetPhrase( textToken.Substring( 1 ), data );
	}

	public static string GetFindFileFilter()
	{
		var sb = new StringBuilder();

		var allproject = GetLocalized(
			"#smix.ui.filter.allproject",
			new()
			{
				["ProjectName"] = SandMix.ProjectName
			}
		);

		sb.Append( allproject );
		sb.Append( " (*." );
		sb.Append( MixGraphResource.FileExtension );
		sb.Append( " *." );
		sb.Append( EffectGraphResource.FileExtension );
		sb.Append( ")" );

		sb.Append( ";;" );

		sb.Append( GetAllFiles() );

		return sb.ToString();
	}

	public static string GetSaveMixGraphFilter()
	{
		var sb = new StringBuilder();

		var mixGraph = GetLocalized(
			"#smix.ui.filter.mixgraph",
			new()
			{
				["ProjectName"] = SandMix.ProjectName
			}
		);

		sb.Append( mixGraph );
		sb.Append( " (*." );
		sb.Append( MixGraphResource.FileExtension );
		sb.Append( ")" );

		sb.Append( ";;" );

		sb.Append( GetAllFiles() );

		return sb.ToString();
	}

	public static string GetSaveEffectGraphFilter()
	{
		var sb = new StringBuilder();

		var effectGraph = GetLocalized(
			"#smix.ui.filter.effectgraph",
			new()
			{
				["ProjectName"] = SandMix.ProjectName
			}
		);

		sb.Append( effectGraph );
		sb.Append( " (*." );
		sb.Append( EffectGraphResource.FileExtension );
		sb.Append( ")" );

		sb.Append( ";;" );

		sb.Append( GetAllFiles() );

		return sb.ToString();
	}

	private static string GetAllFiles()
	{
		return GetLocalized( "#smix.ui.filter.allfiles" ) + " (*.*)";
	}
}
