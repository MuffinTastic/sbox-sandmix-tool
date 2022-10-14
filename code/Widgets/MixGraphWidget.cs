using Sandbox;
using SandMix;
using SandMix.Nodes;
using SandMix.Nodes.Mix;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace SandMix.Tool.Widgets;

public class MixGraphWidget : NodeGraphWidget
{
	public static int NewMixGraphNumber = 0;
	public override int NewWidgetNumber => NewMixGraphNumber;
	public override string FileExtension => MixGraphResource.FileExtension;
	public override string SaveFilter => SandMixTool.SaveMixGraphFilter;

	public MixGraphWidget( Widget parent = null ) : base( GraphType.Mix, "New Mix Graph", MixGraphResource.Icon, parent )
	{
		NewMixGraphNumber++;
	}

	protected override IEnumerable<TypeDescription> NodeTypes => TypeLibrary.GetDescriptions<BaseMixNode>().Where( td => !td.IsAbstract );

	protected override void ReadAssetImpl()
	{
		if ( Asset.TryLoadResource<MixGraphResource>( out var resource ) )
		{
			if ( resource.JsonData is not null )
			{
				GraphView.Graph = GraphContainer.Deserialize( resource.JsonData );
			}
		}
	}

	protected override void WriteAssetImpl()
	{
		if ( Asset.TryLoadResource<MixGraphResource>( out var resource ) )
		{
			resource.JsonData = GraphView.Graph.Serialize();
			Asset.SaveToDisk( resource );
			Asset.Compile( full: false );
		}
	}
}
