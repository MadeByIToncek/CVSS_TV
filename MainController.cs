using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CVSS_GodotCommons;
using Godot;

namespace CVSS_TV;
/* debug if */
#pragma warning disable CS0162 // Unreachable code detected 
public partial class MainController : Control {
	private static readonly Vector2I TwoK = new(2560, 1440);
	private List<Window> _windows = [];
	private List<IResetableControl> _resetable = [];
	private ApiHandler _api = new();
	private WebsocketHandler _wsh;
	private float _fade = 2;
	public MainController() {
		_wsh = new WebsocketHandler(_api);
	}

	public override void _EnterTree() {
		#if DEBUG
		GetWindow().SetMode(Window.ModeEnum.Windowed);
		GetWindow().SetSize(new Vector2I(1920, 1080));
		GetWindow().SetContentScaleSize(TwoK);
		GetWindow().SetCurrentScreen(1);
		GetWindow().SetPosition(new Vector2I(0, 0));
		#else
		GetWindow().SetMode(Window.ModeEnum.ExclusiveFullscreen);
		GetWindow().SetSize(TwoK);
		GetWindow().SetCurrentScreen(0);
		#endif

		AddChild(_api);

		//FullScreenSprite cd = new("res://splash_centrumdeti.png");
		//FullScreenSprite fel = new("res://splash_fel.png");
		//_window.AddChild(cd);
		//AddChild(fel);

		// bool sw = _api.ShouldSwitchTVs();
		ScoreThreeScreenLayout lsc = new(_api, _fade, ScoreThreeScreenLayout.ThreeScreenLayoutType.Left);
		ScoreThreeScreenLayout msc = new(_api, _fade, ScoreThreeScreenLayout.ThreeScreenLayoutType.Time);
		ScoreThreeScreenLayout rsc = new(_api, _fade, ScoreThreeScreenLayout.ThreeScreenLayoutType.Right);
		SpawnWindows(2);

		_resetable.Add(lsc);
		_resetable.Add(msc);
		_resetable.Add(rsc);

		_windows[0].AddChild(rsc);
		_windows[1].AddChild(msc);
		AddChild(lsc);

		GetTree().CreateTimer(6).Timeout += () => {
			foreach (IResetableControl r in _resetable) {
				r.Remove();
			}

			GetTree().CreateTimer(_fade + 1).Timeout += () => {
				RemoveChild(_api);
				_api.QueueFree();
				_wsh.Remove();
				foreach (Window w in _windows) {
					RemoveChild(w);
					w.QueueFree();
				}
				GetTree().Quit();
			};
		};
	}

	[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
	private void SpawnWindows(int i) {
		for (int j = 0; j < i; j++) {
			Window window = new();

			#if DEBUG
			window.Mode = Window.ModeEnum.Windowed;
			window.Size = new Vector2I(1920, 1080);
			window.CurrentScreen = 1;
			#else
				window.Mode = Window.ModeEnum.ExclusiveFullscreen;
				window.Size = TwoK;
				window.CurrentScreen = j + 1;
			#endif

			window.Title = $"CVSS_TV{j + 2}";
			window.InitialPosition = Window.WindowInitialPosition.CenterOtherScreen;
			window.ContentScaleSize = TwoK;
			window.Visible = true;
			window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
			window.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
			_windows.Add(window);
			AddChild(window);

			#if DEBUG
			window.SetPosition(j switch {
				0 => new Vector2I(1920, 0),
				1 => new Vector2I(0, 1080),
				2 => new Vector2I(1920, 1080),
				3 => new Vector2I(3840, 0),
				4 => new Vector2I(5760, 0),
				5 => new Vector2I(3840, 1920),
				_ => new Vector2I(5760, 1920)
			});
			#endif
		}
	}
}

internal interface IResetableControl {
	public void Remove();
}