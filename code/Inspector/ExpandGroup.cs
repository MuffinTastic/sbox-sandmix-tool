using Tools;

namespace SandMixTool.Inspector;

public class ExpandGroup : Widget
{
	Widget widget;

	public string Title { get; set; } = "Untitled Group";
	public string Icon { get; set; } = "feed";

	protected int headerSize;

	public ExpandGroup( Widget parent ) : base( parent )
	{
		SetLayout( LayoutMode.TopToBottom );
		SetHeaderSize( (int)Theme.RowHeight );
		SetSizeMode( SizeMode.Default, SizeMode.Expand );


	}

	public void SetWidget( Widget w )
	{
		widget?.Destroy();
		widget = null;

		widget = w;
		Layout.Add( widget );
	}

	public void SetHeaderSize( int height )
	{
		headerSize = height;
		MinimumSize = height;
		MaximumSize = new( 10000, headerSize );

		Layout.Margin = new( 24, headerSize, 0, 0 );
	}

	protected override void OnPaint()
	{
		bool isExpanded = widget?.Visible ?? false;

		var rect = new Rect( 0, Size );

		rect.height = headerSize;

		Paint.SetPen( Theme.White.WithAlpha( isExpanded ? 0.5f : 0.2f ) );
		var arrowRect = Paint.DrawMaterialIcon( rect, isExpanded ? "arrow_drop_down" : "arrow_right", 22, TextFlag.LeftCenter );

		rect.left += 24;

		Paint.SetDefaultFont( 8, 500 );
		Paint.SetPen( Theme.Grey.Lighten( isExpanded ? 0.6f : 0.4f ) );
		Paint.DrawText( rect, Title, TextFlag.LeftCenter );
	}

	public virtual void SetOpenState( bool state )
	{
		if ( !state && widget == null )
			return;

		if ( state && widget == null )
		{
			OnFirstOpen();
			return;
		}

		widget.Visible = state;
		OnOpenStateChanged( state );
	}

	protected virtual void OnFirstOpen()
	{
		OnOpenStateChanged( true );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LocalPosition.y < headerSize )
		{
			SetOpenState( !(widget?.Visible ?? false) );
		}
	}

	protected virtual void OnOpenStateChanged( bool newState )
	{
		if ( !string.IsNullOrEmpty( StateCookieName ) )
		{
			Cookie.Set( StateCookieName, newState );
		}

		MaximumSize = new( 10000, newState ? 10000 : headerSize );
	}

	string _stateCookieName;
	public string StateCookieName
	{
		get => _stateCookieName;
		set
		{
			if ( _stateCookieName == value ) return;
			_stateCookieName = value;

			var state = Cookie.Get( _stateCookieName, false );
			SetOpenState( state );
		}
	}
}
