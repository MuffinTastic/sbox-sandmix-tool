using Sandbox;
using SandMix;
using SandMix.Nodes;
using SandMix.Nodes.Effects;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMix.Tool.Widgets;

public class EffectGraphWidget : NodeGraphWidget
{
	public static int NewEffectGraphNumber = 0;
	public override int NewWidgetNumber => NewEffectGraphNumber;
	public override string FileExtension => EffectResource.FileExtension;
	public override string SaveFilter => SandMixTool.SaveEffectFilter;

	public EffectGraphWidget( Widget parent = null ) : base( GraphType.Effect, "New Effect", EffectResource.Icon, parent )
	{
		NewEffectGraphNumber++;
	}

	protected override IEnumerable<TypeDescription> NodeTypes => TypeLibrary.GetDescriptions<BaseEffectNode>().Where( td => !td.IsAbstract );

	protected override void ReadAssetImpl()
	{
		if ( Asset.TryLoadResource<EffectResource>( out var resource ) )
		{
			if ( resource.JsonData is not null )
			{
				GraphView.Graph = GraphContainer.Deserialize( resource.JsonData );
			}
		}
	}

	protected override void WriteAssetImpl()
	{
		if ( Asset.TryLoadResource<EffectResource>( out var resource ) )
		{
			resource.JsonData = GraphView.Graph.Serialize();
			Asset.SaveToDisk( resource );
			Asset.Compile( full: false );
		}
	}
}
