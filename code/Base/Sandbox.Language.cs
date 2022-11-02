using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sandbox.Localization;
using Tools;

namespace Sandbox
{
    //
    // Summary:
    //     Allows access to translated phrases, allowing the translation of gamemodes etc
    [SkipHotload]
    public static class Language
    {
        private static PhraseCollection lang;

        private static string _previousLanguage;

        //
        // Summary:
        //     Information about the current selected language. Will default to English if the
        //     current language isn't found.
        public static LanguageInformation Current { get; internal set; }

		[ModuleInitializer]
        internal static void Init()
        {
            _previousLanguage = null;
            Current = null;
            Tick();
        }

		[Event.Frame]
        internal static void Tick()
        {
            string text = ConsoleSystem.GetValue( "language", "en" );
            if ( string.IsNullOrEmpty( text ) )
            {
                text = "en";
            }

            if ( !( _previousLanguage == text ) )
            {
                _previousLanguage = text;
                text.ToLower();
                lang = new PhraseCollection();
                // AddFromPath( "en" );
                // if ( text != "en" )
                // {
                    AddFromPath( text );
				// }

				Event.Run( "language.changed" );
			}
        }

        private static void AddFromPath( string shortName )
        {
            if ( string.IsNullOrWhiteSpace( shortName ) || shortName.Contains( "." ) || shortName.Contains( ":" ) || shortName.Contains( "/" ) )
            {
                return;
            }

            LanguageInformation languageInformation = Languages.Find( shortName );
            if ( languageInformation != null )
            {
                Current = languageInformation;
            }

            shortName = "/.localization/" + shortName;

			// tools compat note: Mounted -> Content
            foreach ( string item in FileSystem.Content.FindFile( shortName, "*.json" ) )
            {
                AddFile( shortName + "/" + item );
            }
        }

        //
        // Summary:
        //     Called when a localization file has changed (and we should reload)
        internal static void OnFileChanged()
        {
            _previousLanguage = null;
        }

        private static void AddFile( string path )
        {
            Dictionary<string, string> dictionary = FileSystem.Mounted.ReadJson<Dictionary<string, string>>( path );
            if ( dictionary == null )
            {
                return;
            }

            foreach ( KeyValuePair<string, string> item in dictionary )
            {
                lang.Set( item.Key, item.Value );
            }
        }

        //
        // Summary:
        //     Lookup a phrase
        //
        // Parameters:
        //   textToken:
        //     The token used to identify the phrase
        //
        // Returns:
        //     If found will return the phrase, else will return the token itself
        public static string GetPhrase( string textToken )
        {
            if ( lang == null )
            {
                return textToken;
            }

            return lang.GetPhrase( textToken );
		}

		//
		// Summary:
		//     Lookup a phrase
		public static string GetPhrase( string textToken, Dictionary<string, object> data )
		{
			if ( lang == null )
			{
				return textToken;
			}

			return lang.GetPhrase( textToken, data );
		}

		internal static void Shutdown()
        {
            lang = null;
        }
    }
}

#if false // Decompilation log
'165' items in cache
------------------
Resolve: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Runtime.dll'
------------------
Resolve: 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.dll'
------------------
Resolve: 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.System.dll'
------------------
Resolve: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Collections.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Reflection.dll'
------------------
Resolve: 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Event.dll'
------------------
Resolve: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.ComponentModel.Primitives.dll'
------------------
Resolve: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Text.Json.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'System.ComponentModel.Annotations, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Annotations, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.ComponentModel.Annotations.dll'
------------------
Resolve: 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Threading.dll'
------------------
Resolve: 'System.Threading.Channels, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Threading.Channels, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Threading.Channels.dll'
------------------
Resolve: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Net.WebSockets.Client.dll'
------------------
Resolve: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Net.WebSockets.dll'
------------------
Resolve: 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
Could not find by name: 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
------------------
Resolve: 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'Sandbox.Bind, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Bind, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Bind.dll'
------------------
Resolve: 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Collections.Immutable.dll'
------------------
Resolve: 'Microsoft.CodeAnalysis, Version=4.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'Microsoft.CodeAnalysis, Version=4.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'Microsoft.CodeAnalysis.CSharp, Version=4.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'Microsoft.CodeAnalysis.CSharp, Version=4.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.ObjectModel.dll'
------------------
Resolve: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Net.Http.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Runtime.CompilerServices.Unsafe.dll'
------------------
Resolve: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Linq.dll'
------------------
Resolve: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Numerics.Vectors.dll'
------------------
Resolve: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Memory.dll'
------------------
Resolve: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Threading.Thread.dll'
------------------
Resolve: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.Console.dll'
------------------
Resolve: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.9\ref\net6.0\System.ComponentModel.EventBasedAsync.dll'
#endif
