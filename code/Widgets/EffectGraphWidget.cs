using Sandbox;
using SandMix;
using SandMix.Effects;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMixTool.Widgets;

public class EffectGraphWidget : NodeGraphWidget
{
	public static int NewEffectGraphNumber = 0;
	public override int NewWidgetNumber => NewEffectGraphNumber;
	public override string FileExtension => EffectResource.FileExtension;
	public override string SaveFilter => SandMixTool.SaveEffectFilter;

	public EffectGraphWidget( Widget parent = null ) : base( effectGraph: true, "New Effect", EffectResource.Icon, parent )
	{
		NewEffectGraphNumber++;
	}

	protected override IEnumerable<TypeDescription> NodeTypes => TypeLibrary.GetDescriptions<BaseEffect>().Where( td => !td.IsAbstract );
}
