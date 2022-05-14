
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable CA2255

#region Assembly Sandbox.Game, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion
namespace Sandbox
{
	//
	// Summary:
	//     Allows the registration/creation of classes by name, in a generic way. Also remembers
	//     class by assembly name, so we can gracefully handle hotloading.
	public static class Library
	{
		private static Logger log = Logging.GetLogger();

		private static readonly ConcurrentDictionary<string, Type> TypeDict = new();

		[ModuleInitializer]
		public static void Initialize()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var types = assembly.GetTypes();

			AddAssembly( assembly, types );
		}

		//
		// Summary:
		//     Called on all loaded assemblies to gather classes with ClassLibrary attributes
		internal static void AddAssembly( Assembly assembly, Type[] types = null )
		{
			if ( types == null )
			{
				types = assembly.GetTypes();
			}

			lock ( TypeDict )
			{
				Type[] array = types;
				foreach ( Type type in array )
				{
					TypeDict[type.Name] = type;
				}
			}
		}

		//
		// Summary:
		//     Get all types that are derived from type
		public static IEnumerable<Type> GetAll<T>()
		{
			lock ( TypeDict )
			{
				return (from x in TypeDict
						where typeof( T ).IsAssignableFrom( x.Value )
						select x.Value).ToList();
			}
		}

		//
		// Summary:
		//     Returns the Type associated with given library name, or null.
		public static Type GetType( string name )
		{
			lock ( TypeDict )
			{
				if ( TypeDict.TryGetValue( name, out Type theType ) )
				{
					return theType;
				}

				return null;
			}
		}
	}
}
#if false // Decompilation log
'166' items in cache
------------------
Resolve: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Runtime.dll'
------------------
Resolve: 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.dll'
------------------
Resolve: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Collections.dll'
------------------
Resolve: 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.System.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'System.Runtime.Loader, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.Loader, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Runtime.Loader.dll'
------------------
Resolve: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Text.Json.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Primitives, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.ComponentModel.Primitives.dll'
------------------
Resolve: 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Event.dll'
------------------
Resolve: 'System.ComponentModel.Annotations, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Annotations, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.ComponentModel.Annotations.dll'
------------------
Resolve: 'Sentry, Version=3.11.1.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0'
Could not find by name: 'Sentry, Version=3.11.1.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0'
------------------
Resolve: 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Threading.dll'
------------------
Resolve: 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
Could not find by name: 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
------------------
Resolve: 'System.Threading.Channels, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Threading.Channels, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Threading.Channels.dll'
------------------
Resolve: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets.Client, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Net.WebSockets.Client.dll'
------------------
Resolve: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.WebSockets, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Net.WebSockets.dll'
------------------
Resolve: 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Collections.Immutable.dll'
------------------
Resolve: 'Microsoft.CodeAnalysis, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'Microsoft.CodeAnalysis, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'Microsoft.CodeAnalysis.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'Microsoft.CodeAnalysis.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Reflection.dll'
------------------
Resolve: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.ObjectModel.dll'
------------------
Resolve: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Http, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Net.Http.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Runtime.CompilerServices.Unsafe.dll'
------------------
Resolve: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Linq.dll'
------------------
Resolve: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Numerics.Vectors, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Numerics.Vectors.dll'
------------------
Resolve: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Memory, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Memory.dll'
------------------
Resolve: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Threading.Thread.dll'
------------------
Resolve: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Console.dll'
------------------
Resolve: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.EventBasedAsync, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.ComponentModel.EventBasedAsync.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.5\ref\net6.0\System.Text.RegularExpressions.dll'
#endif
