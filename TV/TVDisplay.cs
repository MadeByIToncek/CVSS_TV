using CVSS_TV.API;
using Godot;

namespace CVSS_TV.TV;

public class TVDisplay(ApiHandler api, WebsocketHandler wsh, GraphicsInstance i) : IDisplay {
	public void Init(Control parent) {
		throw new System.NotImplementedException();
	}
	public float FadeTime() {
		throw new System.NotImplementedException();
	}
	public void Remove(Control parent) {
		throw new System.NotImplementedException();
	}
	public void Destroy() {
		throw new System.NotImplementedException();
	}
}