#nullable enable
using System;
using System.Threading.Tasks;
using CVSS_TV.API;
using CVSS_TV.Overlay;
using CVSS_TV.TV;
using CVSS_TV.Utils;
using Godot;

namespace CVSS_TV;

public partial class WindowManager(ApiHandler api, WebsocketHandler wsh, int internalInstanceId) : Control{
	private Display _control = new DisplayImpl();
	private double _fadeTime = 0d;
	public override async void _EnterTree() {
		try {
			await ExecuteRefresh();
			wsh.EventReceived += EventHandler;
		}
		catch (Exception e) {
			Console.WriteLine(e.StackTrace);
		}
	}

	private async void EventHandler(object? sender, Event e) {
		try {
			switch (e) {
				case Event.TEAM_UPDATE_EVENT:
				case Event.MATCH_UPDATE_EVENT:
				case Event.MATCH_START:
				case Event.MATCH_RECYCLE:
				case Event.MATCH_END:
				case Event.SCORE_CHANGED:
					break;
				case Event.MATCH_ARM:
				case Event.MATCH_RESET:
				case Event.GRAPHICS_UPDATE_EVENT:
					await ExecuteRefresh();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(e), e, null);
			}
			
		}
		catch (Exception ex) {
			Console.WriteLine(ex.StackTrace);
		}
	}

	private async Task ExecuteRefresh() {
		Console.WriteLine($"Refreshing instance {internalInstanceId}");
		await api.RegisterGraphics(internalInstanceId);
		bool probeMode = await api.GetProbeModeEnabled();
		GraphicsInstance i = await api.GetThisGraphicsInstance(internalInstanceId);
		
		await _control.HideAnimation();
		RemoveChild(_control);
		_control.Delete();
		
		
		if (probeMode) {
			_control = new ProbeDisplay(api, wsh, i);
		}
		else {
			_control = i.Mode switch {
				GraphicsMode.TV_TWO_LEFT => new ScoreTwoScreenLayout(api, wsh, 2f, true),
				GraphicsMode.TV_TWO_RIGHT => new ScoreTwoScreenLayout(api, wsh, 2f, false),
				GraphicsMode.TV_THREE_LEFT => new ScoreThreeScreenLayout(api, wsh, 2f,
					ScoreThreeScreenLayout.ThreeScreenLayoutType.Left),
				GraphicsMode.TV_THREE_RIGHT => new ScoreThreeScreenLayout(api, wsh, 2f,
					ScoreThreeScreenLayout.ThreeScreenLayoutType.Right),
				GraphicsMode.TV_THREE_TIME => new ScoreThreeScreenLayout(api, wsh, 2f,
					ScoreThreeScreenLayout.ThreeScreenLayoutType.Time),
				GraphicsMode.NONE => new DisplayImpl(),
				GraphicsMode.STREAM => new Overlay.Overlay(api, wsh, 2f),
				_ => _control = new DisplayImpl()
			};

			_fadeTime = i.Mode switch {
				GraphicsMode.TV_TWO_LEFT or GraphicsMode.TV_TWO_RIGHT or GraphicsMode.TV_THREE_LEFT
					or GraphicsMode.TV_THREE_RIGHT or GraphicsMode.TV_THREE_TIME or GraphicsMode.STREAM => 2,
				GraphicsMode.NONE => 0,
				_ => 0
			};
		}

		AddChild(_control);
		await _control.Init();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await _control.ShowAnimation();
	}

	public async void Remove() {
		try {
			await _control.HideAnimation();
			RemoveChild(_control);
			_control.Delete();
		}
		catch (Exception e) {
			Console.WriteLine(e.StackTrace);
		}
	}

	public double FadeTime() {
		return _fadeTime;
	}
}