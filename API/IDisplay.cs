using Godot;

namespace CVSS_TV.API;

public interface IDisplay {
	public void Init(Control parent);
	public float FadeTime();
	public void Remove(Control parent);
	public void Destroy();
}