#nullable enable
using System;
using System.Threading.Tasks;
using CVSS_TV.API;
using Godot;

namespace CVSS_TV.Utils;

internal partial class ProbeDisplay(ApiHandler api, WebsocketHandler wsh, GraphicsInstance i)
	: Display {
	private LabelSettings? _ls;
	private Label? _l;

	public override async Task Init() {
		_ls = GenericUtilities.GenerateLabelSettings(200,"res://fonts/internal-bold.ttf");
		_l = new Label {
			Text = i.Ident,
			Position = new Vector2((2560-GenericUtilities.GetStringLength(i.Ident, _ls))/2f,570f),
			LabelSettings = _ls
		};
	}
	public override async Task ShowAnimation() {
		AddChild(_l);
	}
	public override async Task UpdateScore() {
		
	}

	public override async Task HideAnimation() {
		RemoveChild(_l);
		await Task.Delay(1);
	}

	public override void Delete() {
		_l?.QueueFree();
		_ls?.Dispose();
		QueueFree();
	}
}