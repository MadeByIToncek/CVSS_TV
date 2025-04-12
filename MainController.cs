//#define RELEASE

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CVSS_TV.API;
using CVSS_TV.DMX;
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
	private DmxMaster? _dmx;
	private float _fade = 2;
	private readonly List<WindowManager> _windows = [];
	private readonly List<Window> _windowsToDispose = [];

	public MainController() {
		_wsh = new WebsocketHandler(_api);
		AddChild(_wsh);
	}

	public override async void _EnterTree() {
		GetTree().AutoAcceptQuit = false;
		#if DEBUG && !RELEASE
		GetWindow().SetMode(Window.ModeEnum.Windowed);
		GetWindow().SetSize(new Vector2I(1920, 1080));
		GetWindow().SetContentScaleSize(TwoK);
		GetWindow().SetCurrentScreen(1);
		GetWindow().SetPosition(new Vector2I(0, 0));
		GetWindow().SetTitle("CVSS_TV1");
		#else
		GetWindow().SetMode(Window.ModeEnum.ExclusiveFullscreen);
		GetWindow().SetSize(TwoK);
		GetWindow().SetCurrentScreen(0);
		GetWindow().SetTitle("CVSS_TV1");
		GetWindow().SetPosition(new Vector2I(0, 0));
		#endif
		
		if (DisplayServer.GetName() == "headless") {
			await InitDmx();
		}
		else {
			for (var i = 0; i < _api.Cfg.Instances.Length; i++) {
				Node w = i == 0 ? this : SpawnWindow(i);

				WindowManager wm = new(_api, _wsh, i);
				w.AddChild(wm);
				_windows.Add(wm);
			}
		}
	}

	private async Task InitDmx() {
		_dmx = new DmxMaster(_api, _wsh, this);
		await _dmx._Init();
	}

	public override void _Ready() { }



	[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
	private Window SpawnWindow(int i) {
		Window window = new();

		#if DEBUG && !RELEASE
		window.Mode = Window.ModeEnum.Windowed;
		window.Size = new Vector2I(1920, 1080);
		window.CurrentScreen = 1;
		#else
				window.Mode = Window.ModeEnum.ExclusiveFullscreen;
				window.Size = TwoK;
				window.CurrentScreen = i;
		#endif

		window.Title = $"CVSS_TV{i}";
		window.InitialPosition = Window.WindowInitialPosition.CenterOtherScreen;
		window.ContentScaleSize = TwoK;
		window.Visible = true;
		window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
		window.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
		_windowsToDispose.Add(window);
		AddChild(window);

		#if DEBUG && !RELEASE
		window.SetPosition(i switch {
			1 => new Vector2I(1920, 0),
			2 => new Vector2I(0, 1080),
			3 => new Vector2I(1920, 1080),
			4 => new Vector2I(3840, 0),
			5 => new Vector2I(5760, 0),
			6 => new Vector2I(3840, 1920),
			_ => new Vector2I(5760, 1920)
		});
		#endif
		return window;
	}

	public override void _Notification(int what) {
		if (what != NotificationWMCloseRequest) return;

		_windows.ForEach(x=>x.Remove());
		
		double max = _windows.Select(x => x.FadeTime()).Max();
		
		GetTree().CreateTimer(max + .1d).Timeout += () => {
			_windows.ForEach(x=>x.Free());
			_windowsToDispose.ForEach(RemoveChild);
			GetTree().Quit();
		};

	}
}