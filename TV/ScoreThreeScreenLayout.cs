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
	ScoreThreeScreenLayout.ThreeScreenLayoutType type) : Control {
	private static readonly Vector2I TwoK = new(2560, 1440);

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

	public override async void _Ready() {
		if (type == ThreeScreenLayoutType.Time) {
			await InitTimeDisplay();
		}
		else {
			await InitScoreDisplay(type == ThreeScreenLayoutType.Left);
		}
	}

	private async Task InitScoreDisplay(bool left) {
		await InitApi();

		#region Generate singular gradient background
		
		Team t = left ? _leftTeam : _rightTeam;
		_gradient1 = new FullScreenSprite("res://gradient.png", Tween.TransitionType.Sine, fadeDuration / 2f);
		_gradient1.SetModulate(t.ColorBright);
		_gradient1.SetSelfModulate(new Color(0, 0, 0, .5f));
		AddChildAsync(_gradient1);
		
		#endregion

		SpawnLogo(left);
		SpawnTeamName(left, false);

		//todo))
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

	private async Task InitApi() {
		(int ls, int rs) = await api.GetCurrentMatchScore();
		_ls = ls;
		_rs = rs;
		_startTime = TimeSpan.FromSeconds(await api.GetMatchDuration());
		CurrentMatch cm = await api.GetCurrentMatch();
		_leftTeam = cm.LeftTeam;
		_rightTeam = cm.RightTeam;
	}

	private async Task InitTimeDisplay() {
		await InitApi();
		
		#region Generate double gradient background
		
		_gradient1 = new FullScreenSprite("res://gradient_linear.png", Tween.TransitionType.Sine, fadeDuration / 2f);
		_gradient1.SetModulate(_leftTeam.ColorBright);
		AddChildAsync(_gradient1);
		_gradient2 = new FullScreenSprite("res://gradient_linear.png", Tween.TransitionType.Sine, fadeDuration / 2f);
		_gradient2.SetModulate(_rightTeam.ColorBright);
		_gradient2.RotationDegrees = 180;
		_gradient2.Position = TwoK;
		AddChildAsync(_gradient2);
		
		#endregion
		
		SpawnLogo(true);
		SpawnLogo(false);
		SpawnTeamName(true,true);
		SpawnTeamName(false,true);

		SpawnTeamDisplay();
	}

	private void SpawnTeamDisplay() {
		_mainNumber = new Label();
		_mainNumber.SetText($"{_startTime.Minutes:00}:{_startTime.Seconds:00}");
		_mainNumber.SetLabelSettings(_timeSettings);
		var x = 1280f -
		        GenericUtilities.GetStringLength($"{_startTime.Minutes:00}:{_startTime.Seconds:00}", _timeSettings) /
		        2;
		_mainNumber.SetPosition(new Vector2(x, 333));
		AddChildAsync(_mainNumber);
		wsh.TimeReceived += (_, time) => {
			TimeSpan rem = TimeSpan.FromSeconds(time);
			_mainNumber.SetText($"{rem.Minutes:00}:{rem.Seconds:00}");
		};
	}

	public void Remove() {
		_gradient1?.Remove();
		_gradient2?.Remove();

		Tween t = CreateTween();

		t.TweenInterval(fadeDuration).Finished += () => {
			_centrumDeti.Dispose();
			_druhySponzor.Dispose();
			_teamNameSettings.Dispose();
			_scoreSettings.Dispose();
			_timeSettings.Dispose();
			QueueFree();
		};
	}
	private void AddChildAsync(Node node) {
		CallDeferred("add_child", node);
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