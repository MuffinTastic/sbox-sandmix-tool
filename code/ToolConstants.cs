using Sandbox;

namespace SandMix.Tool;

public static class SandMixTool
{
	public const string BaseIcon = "surround_sound";

	public static string GetFindFileFilter()
	{
		var mixGraph = Util.GetLocalized( "#smix.ui.filter.mixgraph" );
		var effectGraph = Util.GetLocalized( "#smix.ui.filter.effectgraph" );

		return
			$"All {SandMix.ProjectName} (*.{MixGraphResource.FileExtension} *.{EffectGraphResource.FileExtension});;" +
			$"{SandMix.ProjectName} {mixGraph} (*.{MixGraphResource.FileExtension});;" +
			$"{SandMix.ProjectName} {effectGraph} (*.{EffectGraphResource.FileExtension});;" +
			$"All files (*.*)";
	}
	public static string GetSaveMixGraphFilter()
	{
		var mixGraph = Util.GetLocalized( "#smix.ui.filter.mixgraph" );

		return
			$"{SandMix.ProjectName} {mixGraph} (*.{MixGraphResource.FileExtension});;" +
			$"All files (*.*)";
	}

	public static string GetSaveEffectGraphFilter()
	{
		var effectGraph = Util.GetLocalized( "#smix.ui.filter.effectgraph" );

		return
			$"{SandMix.ProjectName} {effectGraph} (*.{EffectGraphResource.FileExtension});;" +
			$"All files (*.*)";
	}

	[ConVar.Engine( "smix_tool_debug" )]
	public static bool Debug { get; set; } = false;
}
