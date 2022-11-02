using Sandbox;

namespace SandMix.Tool;

public static class SandMixTool
{
	public const string BaseIcon = "surround_sound";

	[ConVar.Engine( "smix_tool_debug" )]
	public static bool Debug { get; set; } = false;
}
