using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CVSS_TV.Overlay;

public partial class TeamLowerThird(
    string teamName,
    string[] teamMembers,
    Color teamColorBrighter,
    Color teamColorDarker,
    bool left) : Control {
    
    private readonly List<Control> _children = [];
    private LabelSettings _memberLabelSettings = new() {
        FontSize = 85,
        Font = GD.Load<FontFile>("res://fonts/regular.ttf")
    };
    private LabelSettings _teamLabelSettings = new() {
        FontSize = 150,
        Font = GD.Load<FontFile>("res://fonts/regular.ttf")
    };

    private readonly Dictionary<ColorRect, float> _timings = [];
    private ColorRect _mainBox = new();
    private float _mainBoxTime;
    private float _mainBoxTargetY;
    
    public override void _Ready() {
        Tween tw = GetTree().CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        (float x, float _) = _mainBox.Position;
        tw.TweenProperty(this, "position", left ? new Vector2(80, 2060) : new Vector2(3760, 2060),.75f);
        tw.TweenProperty(_mainBox, "position", new Vector2(x, _mainBoxTargetY), _mainBoxTime);
        foreach ((ColorRect rect, float time) in _timings) {
            tw.Parallel().TweenInterval(time).Finished += () => {
                Tween tw2 = GetTree().CreateTween().SetParallel();
                tw2.TweenProperty(rect, "modulate", new Color(1, 1, 1), .3);
            };
        }
    }

    public override void _EnterTree() {
        
        (float mfw, float mfh) = _teamLabelSettings.Font.GetStringSize(teamName,fontSize: _teamLabelSettings.FontSize);
        
        Position = left ? new Vector2(-mfw-200, 2060) : new Vector2(3760+mfw+200, 2060);

        List<string> sort = teamMembers.OrderBy(x => x.Length).ToList();
        
        for (int i = 0; i < sort.Count; i++) {
            string member = sort[i];

            (float fontwidth, float fontheight) = _memberLabelSettings.Font.GetStringSize(member,fontSize: _memberLabelSettings.FontSize);
            
            ColorRect rect = new();
            rect.Size = new Vector2(fontwidth + 30, 125);
            rect.Position = new Vector2(left ? 0 : -rect.Size.X, -125 * (i + 1) - 10 * i);
            rect.Color = teamColorDarker;
            rect.Modulate = new Color(0, 0, 0, 0);
            
            _children.Add(rect);
            AddChild(rect);
            
            Label label = new();
            label.Text = member;
            label.SetLabelSettings(_memberLabelSettings);
            label.Position = new Vector2(15, 5);
            label.Size = new Vector2(fontwidth, fontheight);
        
            _children.Add(label);
            rect.AddChild(label);

            float t = (float)Tween.InterpolateValue(0, 
                sort.Count * .4f, 
                i,
                sort.Count, 
                Tween.TransitionType.Linear,
                Tween.EaseType.InOut);
            
            _timings[rect] = t-.1f;
        }
        _mainBox.Size = new Vector2(mfw + 100, 250);
        _mainBox.Position = new Vector2(left? 0 : -_mainBox.Size.X,-250);
        _mainBox.Color = teamColorBrighter;
            
        _children.Add(_mainBox);
        AddChild(_mainBox);
            
        Label mainLabel = new();
        mainLabel.Text = teamName;
        mainLabel.SetLabelSettings(_teamLabelSettings);
        mainLabel.Position = new Vector2(50, 25);
        mainLabel.Size = new Vector2(mfw, mfh);
        
        _children.Add(mainLabel);
        _mainBox.AddChild(mainLabel);
        _mainBoxTime = sort.Count * .4f;
        _mainBoxTargetY = -125 * sort.Count - 10 * (sort.Count - 1) - 265;
    }

    public void Remove() {
        (float mfw, float _) = _teamLabelSettings.Font.GetStringSize(teamName,fontSize: _teamLabelSettings.FontSize);
        
        Tween tw = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
        (float x, float _) = _mainBox.Position;
        tw.TweenProperty(_mainBox, "position", new Vector2(x, -250), _mainBoxTime).Finished += () => {
            Tween tw2 = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
            tw2.TweenProperty(this, "position", left ? new Vector2(-mfw-200, 2060) : new Vector2(3760+mfw+200, 2060),.75f).Finished +=
                () => {
                    foreach (Control c in _children) {
                        c.GetParent().RemoveChild(c);
                        c.QueueFree();
                    }
                    //GetParent().RemoveChild(this);
                    QueueFree();
                };
        };
        foreach ((ColorRect rect, float time) in _timings.Reverse()) {
            tw.Parallel().TweenInterval(_mainBoxTime - time - .15).Finished += () => {
                Tween tw2 = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
                tw2.TweenProperty(rect, "modulate", new Color(0, 0, 0, 0), .3);
            };
        }
    }
}