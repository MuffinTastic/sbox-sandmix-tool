using Sandbox;

namespace SandMixTool;

public static class SandMixTool
{
	public const string ProjectName = "s&mix";
	public const string ProjectTagline = "Dynamic node-based sound mixer for s&box";
	public const string ProjectVersion = "0.0.1";
	public const string ProjectRepoURL = "https://github.com/MuffinTastic/sbox-sandmix-tool";
	public static readonly string[] Authors = { "MuffinTastic" };

	public const string MixGraphFileExtension = "smix";
	public const string EffectFileExtension = "smixefct";
	public const string FindFileFilter = $"All {ProjectName} (*.{MixGraphFileExtension} *.{EffectFileExtension});;{ProjectName} mixgraph (*.{MixGraphFileExtension});;{ProjectName} effect (*.{EffectFileExtension});;All files (*.*)";
	public const string SaveMixGraphFilter = $"{ProjectName} mixgraph (*.{MixGraphFileExtension});;All files (*.*)";
	public const string SaveEffectFilter = $"{ProjectName} effect (*.{EffectFileExtension});;All files (*.*)";

	[ConVar.Engine( "smix_tool_debug" )]
	public static bool Debug { get; set; } = false;
}
