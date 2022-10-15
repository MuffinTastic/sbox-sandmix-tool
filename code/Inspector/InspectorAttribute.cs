using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Tools;
using Tools.Internal;

namespace SandMix.Tool.Inspector;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public class SandMixInspectorAttribute : Attribute, ITypeAttribute
{
	private static List<SandMixInspectorAttribute> All = new List<SandMixInspectorAttribute>();

	//
	// Summary:
	//     List of attribute types that are generic argument in IEditorAttribute
	private List<Type> EditorAttributeTypes = new List<Type>();

	//
	// Summary:
	//     List of the generic IEditorAttribute class types itself
	private List<Type> EditorAttributes = new List<Type>();

	public Type TargetType { get; set; }

	public Type Type { get; init; }

	public string TypeName { get; init; }

	[ModuleInitializer, Event.Hotload]
	public static void Initialize()
	{
		All.Clear();

		var assembly = Assembly.GetExecutingAssembly();
		var types = assembly.GetTypes();

		var array = types;
		foreach ( Type TargetType in array )
		{
			var attrs = TargetType.GetCustomAttributes<SandMixInspectorAttribute>();
			foreach ( var attr in attrs )
			{
				attr.EditorAttributeTypes.Clear();
				attr.EditorAttributes.Clear();
				attr.TargetType = TargetType;
				Type[] interfaces = TargetType.GetInterfaces();
				foreach ( Type type in interfaces )
				{
					if ( type.IsAssignableTo( typeof( IEditorAttributeBase ) ) )
					{
						Type[] genericTypeArguments = type.GenericTypeArguments;
						if ( genericTypeArguments.Length == 1 )
						{
							attr.EditorAttributeTypes.Add( genericTypeArguments[0] );
							attr.EditorAttributes.Add( type );
						}
					}
				}

				All.Add( attr );
			}
		}
	}

	public static Widget CreateEditorFor( PropertyInfo property, ref object target )
	{
		DisplayInfo propertyDisplayInfo = DisplayInfo.ForMember( property );
		Widget widget = CreateEditorFor( property.PropertyType, property.GetCustomAttributes() );
		if ( widget == null )
		{
			return null;
		}

		PropertyEditorWidget propertyEditorWidget = widget as PropertyEditorWidget;
		if ( propertyEditorWidget != null )
		{
			propertyEditorWidget.SetPropertyDisplayInfo( propertyDisplayInfo );
			propertyEditorWidget.OnReady();
		}

		return widget;
	}

	public static Widget CreateEditorFor( Type t, IEnumerable<Attribute> attributes = null, Type[] generics = null )
	{
		if ( attributes != null )
		{
			Widget widget = CreateWidget( t, attributes, generics );
			if ( widget != null )
			{
				return widget;
			}
		}

		IEnumerable<Attribute> customAttributes = t.GetCustomAttributes();
		Widget widget2 = CreateWidget( t, customAttributes, generics );
		if ( widget2 != null )
		{
			return widget2;
		}

		if ( t.IsGenericType && !t.IsGenericTypeDefinition )
		{
			return CreateEditorFor( t.GetGenericTypeDefinition(), attributes, t.GetGenericArguments() );
		}

		if ( t.IsSZArray )
		{
			return CreateEditorFor( "array", t.GetElementType() );
		}

		if ( t.IsValueType && !t.IsPrimitive && !t.IsEnum )
		{
			return CreateEditorFor( "struct" );
		}

		if ( t.BaseType != null )
		{
			return CreateEditorFor( t.BaseType, attributes );
		}

		return null;
	}

	public static Widget CreateEditorFor( string name )
	{
		foreach ( SandMixInspectorAttribute item in All )
		{
			if ( item.CanEdit( name ) )
			{
				try
				{
					return TypeLibrary.Create<Widget>( item.TargetType, new object[1] );
				}
				catch ( MissingMethodException )
				{
				}
				catch ( Exception ex2 )
				{
					Log.Warning( ex2, $"Error creating {item.TargetType}: {ex2.Message}" );
				}

				try
				{
					return TypeLibrary.Create<Widget>( item.TargetType );
				}
				catch ( MissingMethodException )
				{
				}
				catch ( Exception ex4 )
				{
					Log.Warning( ex4, $"Error creating {item.TargetType}: {ex4.Message}" );
				}
			}
		}

		return null;
	}

	// Dear god
	private static T CreateGeneric<T>( Type type, Type genericArg1, object[] args = null )
	{
		Type type2 = type.MakeGenericType( genericArg1 );
		return (T)Activator.CreateInstance( type2, args );
	}

	internal static Widget CreateEditorFor( string name, Type genericArgument )
	{
		foreach ( SandMixInspectorAttribute item in All )
		{
			if ( item.CanEdit( name ) )
			{
				try
				{
					return CreateGeneric<Widget>( item.TargetType, genericArgument, new object[1] );
				}
				catch ( Exception )
				{
				}
			}
		}

		return null;
	}

	public static Widget CreateEditorForObject( object obj )
	{
		if ( obj == null )
		{
			return null;
		}

		Type type = obj.GetType();
		IEnumerable<Attribute> customAttributes = type.GetCustomAttributes<Attribute>();
		Widget widget = CreateWidget( type, customAttributes, null, obj );
		if ( widget != null )
		{
			return widget;
		}

		if ( type.IsGenericType && !type.IsGenericTypeDefinition )
		{
			return CreateEditorFor( type.GetGenericTypeDefinition(), null, type.GetGenericArguments() );
		}

		return null;
	}

	public SandMixInspectorAttribute( Type type, string typeName = null )
	{
		Type = type;
		TypeName = typeName;
	}

	public SandMixInspectorAttribute( string typeName )
		: this( null, typeName )
	{
	}

	private static Widget CreateWidget( Type t, IEnumerable<Attribute> attributes, Type[] generics = null, object obj = null )
	{
		string name = null;
		if ( t.IsEnum )
		{
			name = "enum";
		}

		if ( t == typeof( bool ) )
		{
			name = "bool";
		}

		SandMixInspectorAttribute canEditAttribute = (from x in All
											 select new
											 {
												 score = x.CanEdit( t, attributes, name ),
												 value = x
											 } into x
											 where x.score > 0
											 orderby x.score descending
											 select x).FirstOrDefault()?.value;
		if ( canEditAttribute == null )
		{
			return null;
		}

		Widget widget = null;
		Type type = canEditAttribute.TargetType;
		if ( type.IsGenericTypeDefinition )
		{
			if ( generics == null )
			{
				generics = new Type[1] { t };
			}

			type = type.MakeGenericType( generics );
			generics = null;
		}

		if ( obj != null )
		{
			try
			{
				widget = TypeLibrary.Create<Widget>( type, new object[2] { null, obj } );
			}
			catch ( MissingMemberException )
			{
				widget = null;
			}
			catch ( Exception exception )
			{
				Log.Warning( exception, $"Couldn't create {type}" );
				widget = null;
			}
		}

		if ( widget == null )
		{
			try
			{
				widget = TypeLibrary.Create<Widget>( type, new object[1] );
			}
			catch ( Exception exception2 )
			{
				Log.Warning( exception2, $"Couldn't create {type}" );
				widget = null;
			}
		}

		for ( int i = 0; i < canEditAttribute.EditorAttributes.Count; i++ )
		{
			Type type2 = canEditAttribute.EditorAttributes[i];
			Type attributeType = canEditAttribute.EditorAttributeTypes[i];
			Attribute attribute = attributes.FirstOrDefault( ( Attribute x ) => x.GetType() == attributeType );
			if ( attribute != null )
			{
				MethodInfo method = type2.GetMethod( "SetEditorAttribute", BindingFlags.Instance | BindingFlags.Public );
				method.Invoke( widget, new object[1] { attribute } );
			}
		}

		return widget;
	}

	private int CanEdit( Type t, IEnumerable<Attribute> attributes, string name )
	{
		Log.Info( $"{t}, {attributes}, {name}" );

		int num = -500;
		if ( EditorAttributeTypes.Count > 0 )
		{
			num -= 5;
			num += attributes.Count( ( Attribute x ) => EditorAttributeTypes.Contains( x.GetType() ) ) * 10;
			num += t.CustomAttributes.Count( ( CustomAttributeData x ) => EditorAttributeTypes.Contains( x.AttributeType ) ) * 10;
		}

		if ( attributes.OfType<EditorAttribute>().Any( ( EditorAttribute x ) => x.Value == TypeName ) )
		{
			return num + 1500;
		}

		if ( Type != null && Type == t )
		{
			return num + 1100;
		}

		if ( !string.IsNullOrEmpty( name ) && string.Equals( TypeName, name, StringComparison.OrdinalIgnoreCase ) )
		{
			return num + 1000;
		}

		if ( Type == null )
		{
			Type[] genericArguments = TargetType.GetGenericArguments();
			foreach ( Type type in genericArguments )
			{
				Type[] genericParameterConstraints = type.GetGenericParameterConstraints();
				if ( genericParameterConstraints.Any( ( Type x ) => t.IsAssignableTo( x ) ) )
				{
					return num + 900;
				}
			}
		}

		if ( Type != null && t.IsAssignableTo( Type ) )
		{
			return num + 800;
		}

		return 0;
	}

	private bool CanEdit( string name )
	{
		return string.Equals( TypeName, name, StringComparison.OrdinalIgnoreCase );
	}
}
