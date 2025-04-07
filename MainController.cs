using CVSS_GodotCommons;
using Godot;

namespace CVSS_TV;

public partial class MainController : Control {
    private static readonly Vector2I TwoK = new(2560, 1440);
    private Window _window;
    private ApiHandler _api = new();
    private float _fade = 2;

    public override void _EnterTree() {
        _window = new Window();
        _window.Mode = Window.ModeEnum.ExclusiveFullscreen;
        _window.Title = "CVSS_TV2";
        _window.InitialPosition = Window.WindowInitialPosition.CenterOtherScreen;
        _window.Size = TwoK;
        _window.CurrentScreen = 1;
        _window.Visible = true;
        _window.ContentScaleSize = TwoK;
        _window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
        _window.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
        AddChild(_window);
        AddChild(_api);

        //FullScreenSprite cd = new("res://splash_centrumdeti.png");
        //FullScreenSprite fel = new("res://splash_fel.png");
        //_window.AddChild(cd);
        //AddChild(fel);
        
        bool sw = _api.ShouldSwitchTVs();
        ScoreScreen lsc = new(!sw, _api, _fade);
        ScoreScreen rsc = new(sw, _api, _fade);
        
        _window.AddChild(lsc);
        AddChild(rsc);
        
        GetTree().CreateTimer(6).Timeout += () => {
            lsc.Remove();
            rsc.Remove();
            //cd.Remove();
            //fel.Remove();
            GetTree().CreateTimer(_fade + 1).Timeout += () => {
                RemoveChild(_api);
                _api.QueueFree();
                RemoveChild(_window);
                _window.QueueFree();
                GetTree().Quit();
            };
        };
    }
    
}