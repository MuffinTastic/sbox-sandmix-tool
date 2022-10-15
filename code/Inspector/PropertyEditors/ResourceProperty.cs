using Sandbox;
using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( "resource" )]
public class ResourceProperty<T> : Widget where T : Resource
{

	T _value;

	public T Value
	{
		get => _value;
		set
		{
			if ( _value == value ) return;

			_value = value;
			_asset = AssetSystem.FindByPath( _value.ResourcePath );
		}
	}

	public ResourceProperty( Widget parent ) : base( parent )
	{
		MinimumSize = Theme.RowHeight * 2;
		Cursor = CursorShape.Finger;
		MouseTracking = true;
		MinimumSize = Theme.RowHeight * 2.0f;
		AcceptDrops = true;

		AssetType = AssetType.FromType( typeof( T ) );

	}

	public string ResourceType;
	public AssetType AssetType;

	Widget AssetExtra;

	Asset _asset;

	public Asset Asset
	{
		get
		{
			return _asset;
		}

		set
		{
			if ( _asset == value )
				return;

			_asset = value;

			if ( _asset == null )
			{
				_value = null;
			}
			else
			{
				_value = _asset.LoadResource<T>() as T;
			}

			SignalValuesChanged();
			Update();
		}
	}


	public void SetEditorAttribute( ResourceTypeAttribute attribute )
	{
		ResourceType = attribute.Editor;

		if ( string.IsNullOrEmpty( ResourceType ) )
		{
			ResourceType = attribute.Type;
			ResourceType = ResourceType.Replace( "resource:", "" );
		}

		// TODO - alternative to ResourceTypeAttribute
		ResourceType = ResourceType.Replace( "AssetBrowse", "" );
		ResourceType = ResourceType.Trim( '(', ')' );

		AssetType = AssetType.Find( ResourceType, false );
		if ( AssetType == null )
		{
			Log.Warning( $"Unknown asset type ({ResourceType}) from  ResourceType( {attribute.Editor} )" );
			return;
		}

		AssetExtra ??= SandMixInspectorAttribute.CreateEditorFor( $"assetextra:{AssetType.FileExtension}" );
		if ( AssetExtra != null )
		{
			AssetExtra.Bind( "Asset" ).From( this, "Asset" );
			AssetExtra.Parent = this;

			DoLayout();
		}
	}

	protected override void DoLayout()
	{
		if ( AssetExtra != null )
		{
			AssetExtra.Update();
			AssetExtra.Position = new Vector2( Width - AssetExtra.Width, (Height - AssetExtra.Height) * 0.5f );
		}
	}

	protected override void OnMouseClick( MouseEvent e )
	{
		base.OnMouseClick( e );

		var picker = new AssetPicker( this, AssetType );

		picker.Window.StateCookie = "ResourceProperty";
		picker.Window.RestoreFromStateCookie();
		picker.Window.Title = $"Select {AssetType.FriendlyName}";
		picker.Value = Asset;
		picker.OnAssetHighlighted = x => Asset = x;
		picker.OnAssetPicked = x => Asset = x;
		picker.Window.Show();
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;

		var rect = new Rect( 0, Size );

		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, 4 );

		//Theme.DrawButton( LocalRect );

		if ( Paint.HasMouseOver )
		{
			Paint.SetPen( Theme.Blue.WithAlpha( 0.8f ), 1 );
			Paint.SetBrush( Theme.Blue.WithAlpha( 0.2f ) );
			Paint.DrawRect( LocalRect.Shrink( 1 ), 4 );
		}

		var iconRect = rect.Shrink( 6 );
		iconRect.Width = iconRect.Height;

		rect.Left = iconRect.Right + 10;

		Paint.ClearPen();
		Paint.SetBrush( Theme.Grey );
		Paint.DrawRect( iconRect.Grow( 1 ), 2 );

		//
		// The object name
		//

		if ( Asset != null && !Asset.IsDeleted )
		{
			Paint.Draw( iconRect, Asset.GetAssetThumb( true ) );

			var textRect = rect.Shrink( 0, 6 );

			Paint.SetPen( Color.White.WithAlpha( 0.9f ) );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( textRect, $"{Asset.Name}", TextFlag.LeftTop );

			Paint.SetDefaultFont();
			Theme.DrawFilename( textRect, Asset.RelativePath, TextFlag.LeftBottom, Color.White.WithAlpha( 0.5f ) );
		}
		else if ( Value != null )
		{
			var textRect = rect.Shrink( 0, 6 );
			Paint.Draw( iconRect, AssetType.Icon64 );
			Paint.SetPen( Color.White.WithAlpha( 0.9f ) );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( textRect, $"Unknown {AssetType.FriendlyName}", TextFlag.LeftTop );

			Paint.SetDefaultFont();
			Theme.DrawFilename( textRect, Value.ResourcePath, TextFlag.LeftBottom, Color.White.WithAlpha( 0.5f ) );
		}
		else if ( AssetType != null )
		{
			Paint.Draw( iconRect, AssetType.Icon64 );
			Paint.SetDefaultFont( italic: true );
			Paint.SetPen( Color.White.WithAlpha( 0.5f ) );
			Paint.DrawText( rect, $"Empty {AssetType.FriendlyName}", TextFlag.LeftCenter );
		}
	}

	public override DropAction OnDragEnter( DragData data )
	{
		if ( !data.HasFileOrFolder )
			return DropAction.Ignore;

		var asset = AssetSystem.FindByPath( data.FileOrFolder );

		if ( asset == null || asset.AssetType != AssetType )
			return DropAction.Ignore;

		return DropAction.Link;
	}

	public override DropAction OnDropEvent( DragData data )
	{
		if ( !data.HasFileOrFolder )
			return DropAction.Ignore;

		var asset = AssetSystem.FindByPath( data.FileOrFolder );

		if ( asset == null || asset.AssetType != AssetType )
			return DropAction.Ignore;

		Asset = asset;
		return DropAction.Link;
	}

	protected override void OnDragStart()
	{
		if ( Asset == null )
			return;

		var drag = new Drag( this );
		drag.Data.Url = new System.Uri( $"file://{Asset.AbsolutePath}" );
		drag.Execute();
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{
		//	if ( Asset == null )
		//		return;

		e.Accepted = true;

		var menu = new Menu();


		menu.AddOption( "Clear", "backspace", () => Value = null );

		if ( Asset != null )
		{
			menu.AddOption( "Open in Editor", "edit", () => Asset.OpenInEditor() );
			// open containing folder
		}

		menu.OpenAt( e.ScreenPosition );

	}
}
