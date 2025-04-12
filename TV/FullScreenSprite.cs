using System.Threading.Tasks;
using Godot;

namespace CVSS_TV.TV;

public partial class FullScreenSprite(
	string path,
	Tween.TransitionType trans = Tween.TransitionType.Linear,
	float fadeDuration = 1f) : Display {
	private TextureRect _rect = new();
	private Texture2D _texture;
	
	public override async Task Init() {
		_texture = GD.Load<Texture2D>(path);
		_rect.Texture = _texture;
		_rect.Position = new Vector2(0, 0);
		_rect.Scale = new Vector2(2560 / _texture.GetSize().X, 1440 / _texture.GetSize().Y);
		_rect.Modulate = new Color(1, 1, 1, 0);
		AddChild(_rect);
	}
	public override async Task ShowAnimation() {
		Tween tween = GetTree().CreateTween().SetTrans(trans);
		tween.TweenProperty(_rect, "modulate", new Color(1, 1, 1), fadeDuration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	public override async Task UpdateScore() {
		
	}

	public override async Task HideAnimation() {
		Tween tween = CreateTween().SetTrans(trans);
		tween.TweenProperty(_rect, "modulate", new Color(1, 1, 1, 0), fadeDuration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	public override void Delete() {
		RemoveChild(_rect);
		_rect?.QueueFree();
		_texture?.Dispose();
	}
}