using Godot;

namespace CVSS_TV;

public partial class FullScreenSprite(
	string path,
	Tween.TransitionType trans = Tween.TransitionType.Linear,
	float fadeDuration = 1f) : Control, IResetableControl {
	private TextureRect _rect = new();
	private Texture2D _texture;

	public override void _Ready() {
		Tween tween = GetTree().CreateTween().SetTrans(trans);
		tween.TweenProperty(_rect, "modulate", new Color(1, 1, 1), fadeDuration);
	}

	public override void _EnterTree() {
		_texture = (Texture2D)GD.Load(path);
		_rect.Texture = _texture;
		_rect.Position = new Vector2(0, 0);
		_rect.Scale = new Vector2(2560 / _texture.GetSize().X, 1440 / _texture.GetSize().Y);
		_rect.Modulate = new Color(1, 1, 1, 0);
		AddChild(_rect);
	}

	public void Remove() {
		Tween tween = GetTree().CreateTween().SetTrans(trans);
		tween.TweenProperty(_rect, "modulate", new Color(1, 1, 1, 0), fadeDuration);
		tween.Finished += () => {
			RemoveChild(_rect);
			_rect.QueueFree();
			_texture.Dispose();
			QueueFree();
		};
	}
}