#nullable enable
using System;
using CVSS_TV.API;
using Godot;

namespace CVSS_TV.Utils;

internal class ProbeDisplay : IDisplay {
	private readonly ApiHandler _api;
	private readonly WebsocketHandler _wsh;

	private readonly LabelSettings _ls;
	private readonly Label _l;
	public ProbeDisplay(ApiHandler api, WebsocketHandler wsh, GraphicsInstance i) {
		_api = api;

		_ls = GenericUtilities.GenerateLabelSettings(200,"res://fonts/internal-bold.ttf");
		
		_l = new Label {
			Text = i.Ident,
			Position = new Vector2((2560-GenericUtilities.GetStringLength(i.Ident, _ls))/2f,570f),
			LabelSettings = _ls
		};
		_wsh = wsh;
	}

	public void Init(Control parent) {
		parent.AddChild(_l);
	}

	public float FadeTime() {
		return 0f;
	}

	public void Remove(Control parent) {
		parent.RemoveChild(_l);
	}
	
	public void Destroy() {
		_l.QueueFree();
		_ls.Dispose();
	}
}