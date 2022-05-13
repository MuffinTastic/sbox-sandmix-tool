using Sandbox;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Tools;

public class PropertySheet : Widget
{
	object _target;
	public object Target
	{
		get => _target;
		set
		{
			if ( object.Equals( _target, value ) )
				return;

			_target = value;
			Rebuild();
		}
	}

	public bool IncludeHeader { get; set; } = false;

	public PropertySheet( Widget parent ) : base( parent )
	{
		SetLayout( LayoutMode.TopToBottom );
		SetSizeMode( SizeMode.Default, SizeMode.CanShrink );
	}

	void Rebuild()
	{
		DestroyChildren();

		if ( Target == null )
			return;

		if ( IncludeHeader )
			Layout.Add( new SheetHeader( this ) );

		var properties = Target.GetType().GetProperties( BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public )
									.Where( x => IsPropertyAcceptable( x ) )
									.Select( x => (property: x, info: DisplayInfo.ForProperty( x )) )
									.Where( x => x.info.Browsable ).ToArray();

		Name = $"{Target.GetType()}";

		var grouped = properties.GroupBy( x => x.info.Group ).ToArray();

		foreach ( var classGroup in grouped )
		{
			var sheet = new PropertySheet( this );

			if ( !string.IsNullOrWhiteSpace( classGroup.Key ) )
			{
				var expander = new ExpandGroup( this );
				expander.Title = classGroup.Key;
				Layout.Add( expander );
				expander.SetWidget( sheet );
				expander.StateCookieName = $"{BuildHierarchyName( expander )}group-{classGroup.Key}-expander";
			}
			else
			{
				Layout.Add( sheet );
			}

			foreach ( var prop in classGroup.OrderBy( x => x.info.Order ) )
			{
				var addedRow = sheet.AddProperty( prop.property, ref _target );

				if ( !string.IsNullOrEmpty( prop.info.Description ) )
				{
					addedRow.StatusTip = prop.info.Description;
					addedRow.ToolTip = prop.info.Description;
				}
			}
		}
	}

	internal void EnableRow( string v, bool visible )
	{
		if ( NamedRows.TryGetValue( v, out var w ) )
		{
			w.Visible = visible;
		}
	}

	Dictionary<string, Widget> NamedRows = new();

	internal void AddProperty( object target, string targetName )
	{
		var prop = target.GetType().GetProperties( BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public )
										.FirstOrDefault( x => x.Name == targetName );

		var info = DisplayInfo.ForProperty( prop );

		var addedRow = AddProperty( prop, ref target );

		if ( !string.IsNullOrEmpty( info.Description ) )
		{
			addedRow.StatusTip = info.Description;
			addedRow.ToolTip = info.Description;
		}
	}

	internal PropertyRow AddDictionaryPropertyInt( Dictionary<string, object> target, string targetName, int defaultValue, int min, int max, string title = null )
	{
		var w = CanEditAttribute.CreateEditorFor( typeof( int ) );
		w.Name = $"{targetName}";
		w.Bind( "Value" ).FromDictionary<string, object>( target, targetName );

		// TODO - apply min max

		var row = new PropertyRow( this );
		row.SetLabel( title ?? targetName );
		row.SetWidget( w );

		return AddRow( row, targetName );
	}

	internal PropertyRow AddDictionaryProperty<T>( Dictionary<string, object> target, string targetName, T defaultValue, string title = null )
	{
		var w = CanEditAttribute.CreateEditorFor( typeof( T ) );
		w.Name = $"{targetName}";
		w.Bind( "Value" ).FromDictionary<string, object>( target, targetName );

		var row = new PropertyRow( this );
		row.SetLabel( title ?? targetName );
		row.SetWidget( w );

		return AddRow( row, targetName );
	}

	public void AddSectionHeader( string name )
	{
		AddRow( new Label( name, null, "PropertyHeader" ) );
	}

	public T AddRow<T>( T row, string name = null ) where T : Widget
	{
		Layout.Add( row );
		row.SetSizeMode( SizeMode.Default, SizeMode.CanShrink );

		if ( name != null )
			NamedRows[name] = row;

		return row;
	}

	public LineEdit AddLineEdit( string title, string value )
	{
		var row = AddRow( new PropertyRow( this ) );
		row.SetLabel( title );
		return row.SetWidget( new LineEdit( value, row ) );
	}

	public TextEdit AddTextEdit( string title, string value )
	{
		var row = AddRow( new PropertyRow( this ) );
		row.SetLabel( title );
		return row.SetWidget( new TextEdit( row ) { PlainText = value } );
	}

	Widget AddProperty( PropertyInfo prop, ref object target )
	{
		var displayInfo = DisplayInfo.ForProperty( prop );

		var w = CanEditAttribute.CreateEditorFor( prop );
		if ( w != null )
		{
			bool fullWidth = false;

			if ( w is IPropertyInspector pi )
			{
				pi.SetDisplayInfo( displayInfo );
				fullWidth = pi.IsFullWidth;
			}

			if ( !fullWidth )
			{
				var row = new PropertyRow( this );
				row.SetLabel( prop );
				row.SetWidget( w );
				AddRow( row, prop.Name );
			}
			else
			{
				AddRow( w, prop.Name );
			}

			w.Name = $"{prop.Name}";

			if ( !prop.CanWrite )
			{
				w.ReadOnly = true;
				w.Bind( "Value" ).ReadOnly().From( target, prop.Name );
			}
			else
			{
				w.Bind( "Value" ).From( target, prop.Name );
			}

			return w;
		}

		// display only
		{
			var row = new PropertyRow( this );
			row.SetLabel( prop );

			/*if ( ObjectProperty.IsApplicable( prop.PropertyType ) )
			{
				//
				// Object drag and droppable field
				//
				var viewer = new ObjectProperty( row, prop.PropertyType );
				viewer.Bind( "Value" ).ReadOnly().From( target, prop.Name );
				row.SetWidget( viewer );
			}
			else*/

			{
				//
				// Generic display 
				//
				var viewer = new Label( row );
				viewer.WordWrap = true;
				viewer.Bind( "Text" ).From( target, prop.Name );
				row.SetWidget( viewer );
			}

			AddRow( row, prop.Name );
			return row;
		}

	}

	public void AddStretch( int i )
	{
		Layout.AddStretchCell( i );
	}

	private bool IsPropertyAcceptable( PropertyInfo x )
	{
		if ( x.GetMethod == null ) return false;
		if ( x.GetMethod.IsStatic ) return false;
		if ( !x.CanRead ) return false;

		if ( x.GetIndexParameters().Length > 0 ) return false;

		if ( x.PropertyType.IsByRefLike ) return false;
		if ( x.GetCustomAttribute<EditorBrowsableAttribute>()?.State == EditorBrowsableState.Never ) return false;
		if ( x.GetCustomAttribute<BrowsableAttribute>()?.Browsable == false ) return false;

		return true;
	}

	public static string BuildHierarchyName( Widget widget )
	{
		var x = "";
		while ( widget != null )
		{
			if ( !string.IsNullOrWhiteSpace( widget.Name ) && !widget.Name.StartsWith( "qt_" ) )
			{
				x = $"{widget.Name}.{x}";
			}

			widget = widget.Parent;
		}

		return x.Trim( '.' );
	}

	public override void ChildValuesChanged( Widget source )
	{
		base.ChildValuesChanged( source );


		Utility.NoteObjectEdited( Target );
	}
}
