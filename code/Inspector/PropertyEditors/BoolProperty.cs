using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;


[SandMixInspector( null, "bool" )]
public class BoolProperty : CheckBox
{
	public BoolProperty( Widget parent ) : base( parent )
	{
		Cursor = CursorShape.Finger;
	}
}
