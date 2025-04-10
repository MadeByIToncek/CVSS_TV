//#define RELEASE

#nullable enable
using System;
using System.Threading.Tasks;
using CVSS_TV.API;
using CVSS_TV.Overlay;
using CVSS_TV.TV;
using CVSS_TV.Utils;
using Godot;
using ApiHandler = CVSS_TV.API.ApiHandler;
using WebsocketHandler = CVSS_TV.API.WebsocketHandler;

namespace CVSS_TV;
/* debug if */
#pragma warning disable CS0162 // Unreachable code detected 
public partial class MainController : Control {
	private static readonly Vector2I TwoK = new(2560, 1440);
	private ApiHandler _api = new();
	private WebsocketHandler _wsh;
	private IDisplay? _d = null;
	private float _fade = 2;

	public MainController() {
		_wsh = new WebsocketHandler(_api);
		AddChild(_wsh);
	}

	public override async void _EnterTree() {
		await ExecuteRefresh();
		_wsh.EventReceived += EventHandler;
	}

	private async void EventHandler(object? sender, Event e) {
		switch (e) {
			case Event.TEAM_UPDATE_EVENT:
			case Event.MATCH_UPDATE_EVENT:
			case Event.MATCH_ARM:
			case Event.MATCH_RESET:
			case Event.MATCH_START:
			case Event.MATCH_RECYCLE:
			case Event.MATCH_END:
			case Event.SCORE_CHANGED:
				break;
			case Event.GRAPHICS_UPDATE_EVENT:
				await ExecuteRefresh();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(e), e, null);
		}
	}

	private async Task ExecuteRefresh() {
		Console.WriteLine("Refresh!");
		await _api.RegisterGraphics();
		bool probeMode = await _api.GetProbeModeEnabled();
		GraphicsInstance i = await _api.GetThisGraphicsInstance();
		if (probeMode) {
			await SpawnDisplay(new ProbeDisplay(_api, _wsh, i));
			return;
		}

		switch (i.Mode) {
			case GraphicsMode.STREAM:
				await SpawnDisplay(new OverlayDisplay(_api,_wsh,i));
				break;
			case GraphicsMode.TV_TWO_LEFT:
			case GraphicsMode.TV_TWO_RIGHT:
			case GraphicsMode.TV_THREE_LEFT:
			case GraphicsMode.TV_THREE_RIGHT:
			case GraphicsMode.TV_THREE_TIME:
				await SpawnDisplay(new TVDisplay(_api,_wsh,i));
				break;
			case GraphicsMode.NONE:
				ClearDisplay();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(i.Mode), i.Mode, "What?");
		}
	}

	private void ClearDisplay() {
		if (_d == null) return;
		
		_d.Remove(this);
		GetTree().CreateTimer(_d.FadeTime()).Timeout += async () => {
			_d.Destroy();
			_d = null;
		};
	}

	private async Task SpawnDisplay(IDisplay display) {
		if (_d != null) {
			_d.Remove(this);
			GetTree().CreateTimer(_d.FadeTime()).Timeout += async () => {
				_d.Destroy();
				_d = null;
				await SpawnDisplay(display);
			};
		}
		else {
			_d = display;
			_d.Init(this);
			await _api.ReportReady();
		}
	}

	public override void _Ready() {
	}
}