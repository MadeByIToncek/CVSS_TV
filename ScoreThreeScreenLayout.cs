using System.Threading.Tasks;
using CVSS_GodotCommons;
using Godot;

namespace CVSS_TV;

#pragma warning disable CS0162
public partial class ScoreThreeScreenLayout(
	ApiHandler api,
	float fadeDuration,
	ScoreThreeScreenLayout.ThreeScreenLayoutType type) : Control, IResetableControl {
	private TextureRect _leftLogo;
	private TextureRect _rightLogo;
	private Label _leftTeamName;
	private Label _rightTeamName;
	private Label _mainNumber;
	private FullScreenSprite _gradient1;
	private FullScreenSprite _gradient2;

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
		//todo))
	}

	private async Task InitTimeDisplay() {
		//TODO))
	}

	public void Remove() {
		_gradient1?.Remove();
		_gradient2?.Remove();

		Tween t = CreateTween();

		t.TweenInterval(fadeDuration).Finished += QueueFree;
	}
}