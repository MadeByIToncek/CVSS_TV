using System;
using System.Threading.Tasks;
using CVSS_TV.API;
using Godot;
using ApiHandler = CVSS_TV.API.ApiHandler;
using WebsocketHandler = CVSS_TV.API.WebsocketHandler;

namespace CVSS_TV.TV;

#pragma warning disable CS0162
public partial class ScoreThreeScreenLayout(
	ApiHandler api,
	WebsocketHandler wsh,
	float fadeDuration,
	ScoreThreeScreenLayout.ThreeScreenLayoutType type) : Display {
	private static readonly Vector2I TwoK = new(2560, 1440);
	
	private bool _disabled = false;

	private Texture2D _centrumDeti = (Texture2D)GD.Load("res://by_centrumdeti.png");
	private Texture2D _druhySponzor = (Texture2D)GD.Load("res://by_centrumdeti.png");
	
	private TextureRect _leftLogo;
	private TextureRect _rightLogo;
	private Label _leftTeamName;
	private Label _rightTeamName;
	private Label _mainNumber;
	private FullScreenSprite _gradient1;
	private FullScreenSprite _gradient2;
	
	private TimeSpan _startTime;
	private Team _leftTeam;
	private Team _rightTeam;
	private int _ls;
	private int _rs;
	
	private LabelSettings _smallTeamNameSettings = GenericUtilities.GenerateLabelSettings();
	private LabelSettings _teamNameSettings = GenericUtilities.GenerateLabelSettings(300);
	private LabelSettings _scoreSettings = GenericUtilities.GenerateLabelSettings(800, "res://fonts/bold.ttf");
	private LabelSettings _timeSettings = GenericUtilities.GenerateLabelSettings(600, "res://fonts/bold.ttf");

	public enum ThreeScreenLayoutType {
		Left,
		Right,
		Time
	}

	private void SpawnTeamName(bool left, bool small) {
		if (left) {
			_leftTeamName = new Label();
			_leftTeamName.SetLabelSettings(small ? _smallTeamNameSettings : _teamNameSettings);
			_leftTeamName.SetText(_leftTeam.Name);
			_leftTeamName.SetPosition(small ? new Vector2(100, 1111) : new Vector2(100, 950));
			AddChildAsync(_leftTeamName);
		}
		else {
			_rightTeamName = new Label();
			_rightTeamName.SetLabelSettings(small ? _smallTeamNameSettings : _teamNameSettings);
			_rightTeamName.SetText(_rightTeam.Name);
			if (small)
				_rightTeamName.SetPosition(new Vector2(
					2460f - GenericUtilities.GetStringLength(_rightTeam.Name,
						_smallTeamNameSettings), 1111));
			else
				_rightTeamName.SetPosition(new Vector2(
					2460f - GenericUtilities.GetStringLength(_rightTeam.Name,
						_teamNameSettings), 950));
			AddChildAsync(_rightTeamName);
		}
	}

	public override async Task Init() {
		(int ls, int rs) = await api.GetCurrentMatchScore();
		_ls = ls;
		_rs = rs;
		_startTime = TimeSpan.FromSeconds(await api.GetMatchDuration());
		CurrentMatch? cm = await api.GetCurrentMatch();
		if (cm == null) {
			_disabled = true;
		}
		else {
			_disabled = false;
			_leftTeam = cm.Value.LeftTeam;
			_rightTeam = cm.Value.RightTeam;
		}


		if(_disabled) return;
		if (type == ThreeScreenLayoutType.Time) {
			#region Generate double gradient background
		
			_gradient1 = new FullScreenSprite("res://gradient_linear.png", Tween.TransitionType.Sine, fadeDuration / 2f);
			_gradient1.SetModulate(_leftTeam.ColorBright);
			AddChildAsync(_gradient1);
			await _gradient1.Init();
			_gradient2 = new FullScreenSprite("res://gradient_linear.png", Tween.TransitionType.Sine, fadeDuration / 2f);
			_gradient2.SetModulate(_rightTeam.ColorBright);
			_gradient2.RotationDegrees = 180;
			_gradient2.Position = TwoK;
			AddChildAsync(_gradient2);
			await _gradient2.Init();
		
			#endregion
		
			SpawnLogo(true);
			SpawnLogo(false);
			SpawnTeamName(true,true);
			SpawnTeamName(false,true);

			SpawnMainNumber(true,false);
		}
		else {
			bool left = type == ThreeScreenLayoutType.Left;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			#region Generate singular gradient background
		
			Team t = left ? _leftTeam : _rightTeam;
			_gradient1 = new FullScreenSprite("res://gradient.png", Tween.TransitionType.Sine, fadeDuration / 2f);
			_gradient1.SetModulate(t.ColorBright);
			_gradient1.SetSelfModulate(new Color(0, 0, 0, .5f));
			await _gradient1.Init();
			AddChildAsync(_gradient1);
		
			#endregion

			SpawnLogo(left);
			SpawnTeamName(left, false);
			SpawnMainNumber(false,left);
		}
	}

	public override async Task ShowAnimation() {
		_gradient1?.ShowAnimation();
		_gradient2?.ShowAnimation();
	}

	public override async Task UpdateScore() { 
		//todo))
	}

	public override async Task HideAnimation()  {
		if(_disabled) return;
		_gradient1?.HideAnimation();
		_gradient2?.HideAnimation();
		
		Tween t = CreateTween().SetParallel();
		if (_leftLogo != null) {
			t.TweenProperty(_leftLogo, "self_modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		}

		if (_rightLogo != null) {
			t.TweenProperty(_rightLogo, "self_modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		}

		if (_leftTeamName != null) {
			t.TweenProperty(_leftTeamName, "self_modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		}

		if (_rightTeamName != null) {
			t.TweenProperty(_rightTeamName, "self_modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		}
		if (_mainNumber != null) {
			t.TweenProperty(_mainNumber, "self_modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		}

		t.TweenInterval(fadeDuration + .1);

		await ToSignal(GetTree().CreateTimer(fadeDuration), SceneTreeTimer.SignalName.Timeout);
		RemoveChildAsync(_leftLogo);
		RemoveChildAsync(_rightLogo);
		RemoveChildAsync(_leftTeamName);
		RemoveChildAsync(_rightTeamName);
		RemoveChildAsync(_mainNumber);
	}

	private void SpawnMainNumber(bool time, bool left) {
		if (time) {
			_mainNumber = new Label();
			_mainNumber.SetText($"{_startTime.Minutes:00}:{_startTime.Seconds:00}");
			_mainNumber.SetLabelSettings(_timeSettings);
			var x = 1280f -
			        GenericUtilities.GetStringLength($"{_startTime.Minutes:00}:{_startTime.Seconds:00}",
				        _timeSettings) /
			        2;
			_mainNumber.SetPosition(new Vector2(x, 333));
			wsh.TimeReceived += (_, t) => {
				TimeSpan rem = TimeSpan.FromSeconds(t);
				_mainNumber.SetText($"{rem.Minutes:00}:{rem.Seconds:00}");
			};
		}
		else {
			_mainNumber = new Label();
			_mainNumber.SetText($"{(left ? _ls : _rs):00}");
			_mainNumber.SetLabelSettings(_scoreSettings);
			_mainNumber.SetPosition(left ? new Vector2(1570, 60) : new Vector2(30, 60));
		}
		AddChildAsync(_mainNumber);
	}

	public override void Delete() {
		_gradient1?.Delete();
		_gradient2?.Delete();
		
		_leftLogo?.QueueFree();
		_rightLogo?.QueueFree();
		_leftTeamName?.QueueFree();
		_rightTeamName?.QueueFree();
		_mainNumber?.QueueFree();
		QueueFree();
		
		_centrumDeti?.Dispose();
		_druhySponzor?.Dispose();
		_teamNameSettings?.Dispose();
		_scoreSettings?.Dispose();
		_timeSettings?.Dispose();
		Dispose();
	}

	private void AddChildAsync(Node node) {
		CallDeferred("add_child", node);
	}
	private void RemoveChildAsync(Node node) {
		CallDeferred("remove_child", node);
	}

	private void SpawnLogo(bool left) {
		if (left) {
			_leftLogo = new TextureRect();
			_leftLogo.SetTexture(_centrumDeti);
			_leftLogo.SetPosition(new Vector2(80, 70));
			_leftLogo.SetScale(new Vector2(0.33333333f, 0.33333333f));
			AddChildAsync(_leftLogo);
		}
		else {
			_rightLogo = new TextureRect();
			_rightLogo.SetTexture(_centrumDeti);
			_rightLogo.SetPosition(new Vector2(1626.6667f, 70));
			_rightLogo.SetScale(new Vector2(0.33333333f, 0.33333333f));
			AddChildAsync(_rightLogo);
		}
	}
}