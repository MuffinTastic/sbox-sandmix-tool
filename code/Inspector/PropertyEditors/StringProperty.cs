using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

[SandMixInspector( typeof( string ), "string" )]
public class StringProperty : LineEdit
{
	public StringProperty( Widget parent ) : base( parent )
	{
		MinimumSize = Theme.RowHeight;
		MaximumSize = new Vector2( 4096, Theme.RowHeight );
	}
}
