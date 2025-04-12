using System;
using System.Threading.Tasks;
using CVSS_TV.API;
using Godot;
using ApiHandler = CVSS_TV.API.ApiHandler;

namespace CVSS_TV.TV;

public partial class ScoreTwoScreenLayout(ApiHandler api, WebsocketHandler wsh, float fadeDuration, bool left) : Display {
	private Texture2D _logoTexture;
	private Label _score = new();
	private Label _teamName = new();
	private Label _timePart = new();
	private Label _doubleDot = new();
	private FullScreenSprite _glow = new("res://gradient.png", Tween.TransitionType.Sine, fadeDuration / 2f);
	private TextureRect _logo = new();

	private LabelSettings _teamNameSettings = GenericUtilities.GenerateLabelSettings();
	private LabelSettings _scoreSettings = GenericUtilities.GenerateLabelSettings(500, "res://fonts/bold.ttf");
	private LabelSettings _timeSettings = GenericUtilities.GenerateLabelSettings(700, "res://fonts/bold.ttf");

	private int _scoreNumber;
	private TimeSpan _startTime;
	private Team _team;
	private bool _disabled;
	public override async void _Ready() {
		try {
		}
		catch (Exception e) {
			await Console.Error.WriteLineAsync(e.StackTrace);
		}
	}

	private async Task SetupVariables() {
		(int ls, int rs) = await api.GetCurrentMatchScore();
		_scoreNumber = left ? ls : rs;
		_startTime = TimeSpan.FromSeconds(await api.GetMatchDuration());
		CurrentMatch? cm = await api.GetCurrentMatch();
		if (cm == null) {
			GD.PrintErr("Unable to find current match on server!");
			_disabled = true;
		}
		else {
			_disabled = false;
			_team = left ? cm.Value.LeftTeam : cm.Value.RightTeam;
		}
	}

	public override async Task Init() {
		await SetupVariables();

		if (_disabled) return;

		_glow.SetModulate(_team.ColorBright);

		// Logo
		_logoTexture = left
			? (Texture2D)GD.Load("res://by_centrumdeti.png")
			: (Texture2D)GD.Load("res://splash_fel.png");
		_logo.SetTexture(_logoTexture);
		_logo.SetPosition(left ? new Vector2(50, 60) : new Vector2(1856, 40));
		_logo.SetScale(left
			? new Vector2(1 / 4f, 1 / 4f)
			: new Vector2(659f / _logoTexture.GetWidth(), 659f / _logoTexture.GetWidth()));
		_logo.SetModulate(new Color(1, 1, 1, 0));

		// Team name
		_teamName.SetText(_team.Name);
		_teamName.SetLabelSettings(_teamNameSettings);
		_teamName.SetPosition(left
			? new Vector2(-100 - GenericUtilities.GetStringLength(_team.Name, _teamNameSettings), 1111)
			: new Vector2(2660, 1111));

		// Score
		_score.SetText($"{_scoreNumber:00}");
		_score.SetLabelSettings(_scoreSettings);
		_score.SetPosition(left
			? new Vector2(-100 - GenericUtilities.GetStringLength($"{_scoreNumber:00}", _scoreSettings), 500)
			: new Vector2(2660, 500));

		_timePart.SetText($"{(left ? _startTime.Minutes : _startTime.Seconds):00}");
		_timePart.SetLabelSettings(_timeSettings);
		_timePart.SetPosition(left ? new Vector2(1510, 222) : new Vector2(210, 222));
		_timePart.SetModulate(new Color(1, 1, 1, 0));

		_doubleDot.SetText(":");
		_doubleDot.SetLabelSettings(_timeSettings);
		_doubleDot.SetPosition(left ? new Vector2(2340, 222) : new Vector2(-200, 222));
		_doubleDot.SetModulate(new Color(1, 1, 1, 0));

		AddChildAsync(_glow);
		AddChildAsync(_logo);
		AddChildAsync(_teamName);
		AddChildAsync(_score);
		AddChildAsync(_timePart);
		AddChildAsync(_doubleDot);
		await _glow.Init();
		// Glow
	}
	
	public override async Task ShowAnimation() {
		if(_disabled) return;
		_glow.ShowAnimation();
		Tween t = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		//Target teamname
		t.TweenProperty(_teamName, "position",
			left ? new Vector2(100, 1111) : new Vector2(2460 - GenericUtilities.GetStringLength(_team.Name, _teamNameSettings), 1111),
			fadeDuration / 2);
		t.Parallel().TweenProperty(_score, "position", left
			? new Vector2(100, 500)
			: new Vector2(2460 - GenericUtilities.GetStringLength($"{_scoreNumber:00}", _scoreSettings), 500), fadeDuration / 2);
		t.Parallel().TweenProperty(_logo, "modulate", new Color(1, 1, 1), fadeDuration / 2);
		t.Parallel().TweenInterval(fadeDuration / 2 - fadeDuration / 10);
		t.TweenProperty(_timePart, "modulate", new Color(1, 1, 1), fadeDuration / 2);
		t.Parallel().TweenProperty(_doubleDot, "modulate", new Color(1, 1, 1), fadeDuration / 2);
		
		await ToSignal(t, Tween.SignalName.Finished);
	}

	public override async Task UpdateScore() {
		//todo))
	}

	public override async Task HideAnimation() {
		if(_disabled) return;
		_glow.HideAnimation();
		Tween t = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
		t.TweenProperty(_timePart, "modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		t.Parallel().TweenProperty(_doubleDot, "modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		t.Parallel().TweenProperty(_logo, "modulate", new Color(1, 1, 1, 0), fadeDuration / 2);
		t.Parallel().TweenInterval(fadeDuration / 2 - fadeDuration / 10);
		t.TweenProperty(_teamName, "position",
			left
				? new Vector2(-100 - GenericUtilities.GetStringLength(_team.Name, _teamNameSettings), 1111)
				: new Vector2(2660, 1111),
			fadeDuration / 2);
		t.Parallel().TweenProperty(_score, "position", left
			? new Vector2(-100 - GenericUtilities.GetStringLength($"{_scoreNumber:00}", _scoreSettings), 500)
			: new Vector2(2660, 500), fadeDuration / 2);
		
		await ToSignal(GetTree().CreateTimer(fadeDuration), SceneTreeTimer.SignalName.Timeout);
		RemoveChildAsync(_glow);
		RemoveChildAsync(_logo);
		RemoveChildAsync(_teamName);
		RemoveChildAsync(_score);
		RemoveChildAsync(_timePart);
		RemoveChildAsync(_doubleDot);
	}

	private void AddChildAsync(Node node) {
		CallDeferred("add_child", node);
	}
	private void RemoveChildAsync(Node node) {
		CallDeferred("remove_child", node);
	}

	public override void Delete() {
		_glow?.Delete();
		_logoTexture?.Dispose();
		_teamNameSettings?.Dispose();
		_timeSettings?.Dispose();
		_scoreSettings?.Dispose();
		QueueFree();
	}
}