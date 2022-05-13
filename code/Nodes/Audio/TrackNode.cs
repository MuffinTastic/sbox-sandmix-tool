using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sandbox;
using SandMixTool.NodeGraph;
using Tools;

namespace SandMixTool.Nodes.Audio;

[Library, Display( Name = "Track", Description = "Audio track - Sources from a .wav file", GroupName = "Audio" )]
public class TrackNode : BaseNode
{
	[ResourceType( "vsnd" )]
	public string Track { get; set; }

	[Browsable( false )]
	public string TrackFile => AssetSystem.FindByPath( Track )?.RelativePath;

	[Browsable( false ), Output]
	public Types.Audio Output { get; set; }
}
