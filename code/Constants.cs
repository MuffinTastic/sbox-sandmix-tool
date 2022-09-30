namespace SandMixTool;

public static class SandMixTool
{
	public const string ProjectName = "s&mix";
	public const string ProjectTagline = "Dynamic node-based sound mixer for s&box";
	public const string ProjectVersion = "0.0.1";
	public const string ProjectRepoURL = "https://github.com/MuffinTastic/sbox-sandmix-tool";
	public static readonly string[] Authors = { "MuffinTastic" };

	public const string FileExtension = "smix";
	public const string FileFilter = $"{ProjectName} (*.{FileExtension});;All files (*.*)";

	public static bool Debug { get; set; } = false;
}
