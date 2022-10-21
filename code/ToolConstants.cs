using Sandbox;

namespace SandMix.Tool;

public static class SandMixTool
{
	public const string BaseIcon = "surround_sound";

	public static string GetFindFileFilter()
	{
		return Util.GetLocalized(
			"#smix.ui.filter.all",
			new()
			{
				["ProjectName"] = SandMix.ProjectName,
				["MixGraphFileExtension"] = MixGraphResource.FileExtension,
				["EffectGraphFileExtension"] = EffectGraphResource.FileExtension
			}
		);
	}
	public static string GetSaveMixGraphFilter()
	{
		return Util.GetLocalized(
			"#smix.ui.filter.mixgraph",
			new()
			{
				["ProjectName"] = SandMix.ProjectName,
				["MixGraphFileExtension"] = MixGraphResource.FileExtension
			}
		);
	}

	public static string GetSaveEffectGraphFilter()
	{
		return Util.GetLocalized(
			"#smix.ui.filter.effectgraph",
			new()
			{
				["ProjectName"] = SandMix.ProjectName,
				["EffectGraphFileExtension"] = EffectGraphResource.FileExtension
			}
		);
	}

	[ConVar.Engine( "smix_tool_debug" )]
	public static bool Debug { get; set; } = false;
}
