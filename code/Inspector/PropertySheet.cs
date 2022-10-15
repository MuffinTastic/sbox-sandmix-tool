using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector;

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

	public object Value
	{
		get => Target;
		set => Target = value;
	}

	public bool IncludeHeader { get; set; } = false;

	public Layout BodyLayout { get; }

	public PropertySheet( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();
		BodyLayout = Layout.AddColumn();
		Layout.AddStretchCell();
	}

	public ExpandGroup CreateExpander( string title )
	{
		var expander = new ExpandGroup( this );
		expander.Title = title;
		BodyLayout.Add( expander );
		expander.StateCookieName = $"{BuildHierarchyName( expander )}group-{title}-expander";
		return expander;
	}

	void Rebuild()
	{
		OnUpdateRowVisibility = null;
		BodyLayout.Clear( true );

		if ( Target == null )
			return;

		if ( IncludeHeader )
			BodyLayout.Add( new SheetHeader( this ) );

		var type = Target.GetType();

		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy )
									.Where( IsPropertyAcceptable )
									.Select<PropertyInfo, (PropertyInfo prop, DisplayInfo info)>( x => new( x, DisplayInfo.ForMember( x ) ) );

		Name = $"{Target.GetType()}";

		var grouped = properties.GroupBy( x => x.info.Group ).ToArray();

		foreach ( var classGroup in grouped )
		{
			var sheet = new PropertySheet( this );
			sheet._target = _target;

			// Force all child rows to refresh their visibility state via ConditionalVisibility
			OnUpdateRowVisibility += () => sheet.ChildValuesChanged( null );

			if ( !string.IsNullOrWhiteSpace( classGroup.Key ) )
			{
				var expander = CreateExpander( classGroup.Key );
				expander.SetWidget( sheet );
				sheet.ContentMargins = 16;
			}
			else
			{
				BodyLayout.Add( sheet );
			}

			foreach ( var prop in classGroup.OrderBy( x => x.info.Order ) )
			{
				var addedRow = sheet.AddProperty( prop.prop, ref _target );

				if ( !string.IsNullOrEmpty( prop.info.Description ) )
				{
					addedRow.StatusTip = prop.info.Description;
					addedRow.ToolTip = prop.info.Description;
				}
			}
		}

		AdjustSize();
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
		var prop = target.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault( x => x.Name == targetName );
		if ( prop == null ) return;

		var addedRow = AddProperty( prop, ref target );
	}

	internal PropertyRow AddDictionaryPropertyInt( Dictionary<string, object> target, string targetName, int defaultValue, int min, int max, string title = null )
	{
		var w = SandMixInspectorAttribute.CreateEditorFor( typeof( int ) );
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
		var w = SandMixInspectorAttribute.CreateEditorFor( typeof( T ) );
		w.Name = $"{targetName}";
		w.Bind( "Value" ).FromDictionary<string, object>( target, targetName );

		var row = new PropertyRow( this );
		row.SetLabel( title ?? targetName );
		row.SetWidget( w );

		return AddRow( row, targetName );
	}

	public void AddSectionHeader( string name )
	{
		var label = AddRow( new Label( name, null ) );
		label.Name = "PropertyHeader";
	}

	public T AddRow<T>( T row, string name = null ) where T : Widget
	{
		BodyLayout.Add( row );
		row.SetSizeMode( SizeMode.Default, SizeMode.CanShrink );

		if ( name != null )
			NamedRows[name] = row;

		return row;
	}

	public T AddRow<T>( T row, PropertyInfo prop ) where T : Widget
	{
		var visibilityAttributes = prop.GetCustomAttributes<ConditionalVisibilityAttribute>().ToArray();

		if ( visibilityAttributes.Length > 0 )
		{
			var f = () =>
			{
				if ( Target == null )
					return;

				row.Visible = true;

				foreach ( var attr in visibilityAttributes )
				{
					if ( !attr.TestCondition( Target, TypeLibrary.GetDescription( Target.GetType() ) ) )
						row.Visible = false;
				}
			};

			OnUpdateRowVisibility += f;

			f();
		}

		return AddRow( row, prop.Name );
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
		var di = DisplayInfo.ForMember( prop );
		var w = SandMixInspectorAttribute.CreateEditorFor( prop, ref target );
		if ( w != null )
		{
			var row = new PropertyRow( this );
			row.SetLabel( di );
			row.SetWidget( w );

			row.ToolTip = di.Description;
			row.StatusTip = di.Description;
			AddRow( row, prop );

			w.Name = $"{prop.Name}";

			if ( !prop.CanWrite )
			{
				w.ReadOnly = true;
				w.Bind( "Value" ).ReadOnly().From( target, prop );
			}
			else
			{
				w.Bind( "Value" ).From( target, prop );
			}

			return w;
		}

		bool childPropertySheet = prop.PropertyType.IsClass;
		childPropertySheet = prop.PropertyType.IsValueType;

		// class/struct
		if ( childPropertySheet )
		{
			var t = target;
			var expander = new ExpandGroup( this );
			expander.Title = di.Name;
			expander.OnCreateWidget += () =>
			{
				var pg = new PropertySheet( expander );
				pg.ContentMargins = 16;
				expander.SetWidget( pg );

				var targetValue = prop.GetValue( t );

				// if the class if null, create a new instance of it
				try
				{
					if ( targetValue == null )
					{
						prop.SetValue( t, System.Activator.CreateInstance( prop.PropertyType ) );
					}
				}
				catch ( System.Exception )
				{

				}

				pg.Bind( "Target" ).From( t, prop );
			};
			BodyLayout.Add( expander );
			expander.StateCookieName = $"{BuildHierarchyName( expander )}group-{prop.DeclaringType.Name}-{prop.Name}-expander";
			return expander;
		}

		// display only
		{
			var row = new PropertyRow( this );
			row.SetLabel( di );

			if ( ObjectProperty.IsApplicable( prop.PropertyType ) )
			{
				//
				// Object drag and droppable field
				//
				var viewer = new ObjectProperty( row, prop.PropertyType );
				viewer.Bind( "Value" ).ReadOnly().From( target, prop );
				row.SetWidget( viewer );
			}
			else
			{
				//
				// Generic display
				//
				var viewer = new Label( row );
				viewer.WordWrap = true;
				viewer.Bind( "Text" ).From( target, prop );
				row.SetWidget( viewer );
			}

			AddRow( row, prop );
			return row;
		}

	}

	private bool IsPropertyAcceptable( PropertyInfo x )
	{
		if ( !x.CanRead ) return false;
		if ( x.GetMethod.IsStatic ) return false;

		var info = DisplayInfo.ForMember( x );

		//if ( x.GetIndexParameters().Length > 0 ) return false;
		//if ( x.PropertyType.IsByRefLike ) return false;

		if ( info.HasTag( "hideineditor" ) ) return false;

		if ( x.GetCustomAttributes<BrowsableAttribute>().Any( x => x.Browsable == false ) ) return false;
		if ( x.GetCustomAttributes<EditorBrowsableAttribute>().Any( x => x.State == EditorBrowsableState.Never ) ) return false;

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

	System.Action OnUpdateRowVisibility;

	public System.Action PropertyUpdated;

	public override void ChildValuesChanged( Widget source )
	{
		base.ChildValuesChanged( source );

		BindSystem.Flush();
		OnUpdateRowVisibility?.Invoke();
		PropertyUpdated?.Invoke();

		Utility.NoteObjectEdited( Target );
	}
}
