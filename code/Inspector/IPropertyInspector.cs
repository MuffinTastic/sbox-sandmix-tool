using Sandbox;

namespace SandMixTool.Inspector;

public interface IPropertyInspector
{
	public bool IsFullWidth => false;
	public void SetDisplayInfo( DisplayInfo display ) { }
}
