#nullable enable
using System.Threading.Tasks;
using Godot;

namespace CVSS_TV;

public abstract partial class Display : Control {
	public abstract Task Init();
	public abstract Task ShowAnimation();
	public abstract Task UpdateScore();
	public abstract Task HideAnimation();
	public new abstract void Delete();
}

public partial class DisplayImpl : Display {
	public override async Task Init() {
		
	}
	public override async Task ShowAnimation() {
		
	}
	public override async Task UpdateScore() {
		
	}

	public override async Task HideAnimation() {
		await Task.Delay(1);
	}

	public override void Delete() {
		QueueFree();
	}
}