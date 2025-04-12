using System.Threading.Tasks;
using CVSS_TV.API;
using Godot;

namespace CVSS_TV.Overlay;

public partial class Overlay(ApiHandler api, WebsocketHandler wsh, float fadeTime) : Display {
	public override async Task Init() { }
	public override async Task ShowAnimation() { }
	public override async Task UpdateScore() { }

	public async override Task HideAnimation() { }

	public override void Delete() {
		QueueFree();
	}
}