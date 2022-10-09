using Sandbox;
using SandMix;
using SandMix.Nodes.Mix;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMixTool.Widgets;

public class MixGraphWidget : NodeGraphWidget
{
	public static int NewMixGraphNumber = 0;
	public override int NewWidgetNumber => NewMixGraphNumber;
	public override string FileExtension => MixGraphResource.FileExtension;
	public override string SaveFilter => SandMixTool.SaveMixGraphFilter;

	public MixGraphWidget( Widget parent = null ) : base( effectGraph: false, "New Mix Graph", MixGraphResource.Icon, parent )
	{
		NewMixGraphNumber++;
	}

	protected override IEnumerable<TypeDescription> NodeTypes => TypeLibrary.GetDescriptions<BaseMixNode>().Where( td => !td.IsAbstract );
}
