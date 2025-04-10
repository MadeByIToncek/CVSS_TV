using System;
using System.Collections.Generic;
using Godot;

namespace CVSS_TV.Overlay;

public partial class TimerController (Color team1, Color team2) : Control {
    private ColorRect _timerBackground;
    private Label _timerLabel;
    private ColorRect _t1Box;
    private Label _t1Label;
    private ColorRect _t2Box;
    private Label _t2Label;

    private readonly List<Control> _children = [];
    private LabelSettings _defaultLabelSettings = new() {
        FontSize = 100,
        Font = GD.Load<FontFile>("res://fonts/regular.ttf")
    };

    public override void _Ready() {
        Tween tw = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);

        tw.TweenProperty(this, "position", new Vector2(1720.0f, 60.0f), 1f);
        tw.TweenInterval(0.809016994374947d).Finished += () => {
            Tween tw2 = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);
            
            tw2.TweenProperty(_t1Box, "position", new Vector2(-200f, 0f), 1f);
            tw2.TweenProperty(_t2Box, "position", new Vector2(+400f, 0f), 1f);    
        };
    }

    public override void _EnterTree() {
        Position = new Vector2(1720, -200);
        
        //----------- T1 ----------------
        _t1Box = new();
        _t1Box.Position = new Vector2(0, 0);
        _t1Box.Size = new Vector2(200, 133);
        _t1Box.Color = new Color(team1);
        
        _children.Add(_t1Box);
        AddChild(_t1Box);
        
        _t1Label = new();
        _t1Label.Text = "01";
        _t1Label.SetLabelSettings(_defaultLabelSettings);
        _t1Label.Position = new Vector2(40, 0);
        _t1Label.Size = new Vector2(120, 133.333f);
        
        _children.Add(_t1Label);
        _t1Box.AddChild(_t1Label);
        
        
        //----------- T2 ----------------
        _t2Box = new();
        _t2Box.Position = new Vector2(200, 0);
        _t2Box.Size = new Vector2(200, 133);
        _t2Box.Color = new Color(team2);
        
        _children.Add(_t2Box);
        AddChild(_t2Box);
        
        
        _t2Label = new();
        _t2Label.Text = "99";
        _t2Label.SetLabelSettings(_defaultLabelSettings);
        _t2Label.Position = new Vector2(40, 0);
        _t2Label.Size = new Vector2(120, 133.333f);
        
        _children.Add(_t2Label);
        _t2Box.AddChild(_t2Label);
        
        _timerBackground = new();
        _timerBackground.Position = new Vector2(0, 0);
        _timerBackground.Size = new Vector2(400, 133);
        _timerBackground.Color = new Color("#101010");
        
        _children.Add(_timerBackground);
        AddChild(_timerBackground);
        
        _timerLabel = new();
        _timerLabel.Text = "01:00";
        _timerLabel.SetLabelSettings(_defaultLabelSettings);
        _timerLabel.Position = new Vector2(50, 0);
        _timerLabel.Size = new Vector2(300, 133.333f);
        
        _children.Add(_timerLabel);
        AddChild(_timerLabel);
    }

    public void Remove() {
        Tween tw2 = GetTree().CreateTween().SetTrans(Tween.TransitionType.Cubic);
        
        tw2.Parallel().TweenProperty(_t1Box, "position", new Vector2(0f, 0f), 1f);
        tw2.Parallel().TweenProperty(_t2Box, "position", new Vector2(200f, 0f), 1f);
        tw2.Parallel().TweenInterval(0.809016994374947d);
        tw2.TweenProperty(this, "position", new Vector2(1720.0f, -200.0f), 1f);

        tw2.Finished += () => {
            foreach (Control c in _children) {
                c.QueueFree();
            }
            QueueFree();
        };
    }

    public void SetCurrentTime(int i) {
        TimeSpan rem = TimeSpan.FromSeconds(i);
        _timerLabel.SetText($"{rem.Minutes:00}:{rem.Seconds:00}");
    }
}