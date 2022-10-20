using Sandbox;

namespace SandMix.Tool;

public static class SandMixTool
{
	public const string BaseIcon = "surround_sound";

	public const string FindFileFilter =
		$"All {SandMix.ProjectName} (*.{MixGraphResource.FileExtension} *.{EffectGraphResource.FileExtension});;" +
		$"{SandMix.ProjectName} mixgraph (*.{MixGraphResource.FileExtension});;" +
		$"{SandMix.ProjectName} effect (*.{EffectGraphResource.FileExtension});;" +
		$"All files (*.*)";

	public const string SaveMixGraphFilter =
		$"{SandMix.ProjectName} mixgraph (*.{MixGraphResource.FileExtension});;" +
		$"All files (*.*)";

	public const string SaveEffectFilter =
		$"{SandMix.ProjectName} effect (*.{EffectGraphResource.FileExtension});;" +
		$"All files (*.*)";

	[ConVar.Engine( "smix_tool_debug" )]
	public static bool Debug { get; set; } = false;
}
