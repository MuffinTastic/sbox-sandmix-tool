using Tools;

namespace SandMix.Tool.Inspector.PropertyEditors;

/// <summary>
/// A draggable slider for integers with a text entry for manual input on the right.
/// </summary>
[SandMixInspector( typeof( int ), "int" )]
public class IntSliderProperty : FloatSliderProperty
{
	public IntSliderProperty( Widget parent ) : base( parent )
	{

	}
}
