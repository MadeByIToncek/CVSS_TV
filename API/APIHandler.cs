#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient;
namespace CVSS_TV.API;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")] // no, it can't, this is made to be public across the app!
public partial class ApiHandler : Control {
    public readonly Config Cfg;
    private readonly HttpClient _scl;

    public ApiHandler() {
        if (!File.Exists("api.config")) {
            GenerateConfig("api.config");
        }
        Config? cfg = ReadConfig("api.config");

        Cfg = cfg ?? throw new Exception("Config couldn't be read!");
        _scl = new HttpClient
        {
            BaseAddress = new Uri(Cfg.BaseUrl)
        };
    }

    private static Config? ReadConfig(string f) { 
        using FileStream stream = File.OpenRead(f);
        return JsonSerializer.Deserialize<Config>(stream);
    }

    private static void GenerateConfig(string cfg) {
        using FileStream stream = File.Create(cfg);
        JsonSerializer.Serialize(stream, new Config {
            BaseUrl = "http://localhost:4444",
            Instances = [
                $"instance-{new Random().Next():x8}"
            ]
        });
    }

    public override void _Ready() {
        GD.Print(GetServerVersion());
    }

    public override void _EnterTree() {
        
    }

    public void Remove() {
        _scl.Dispose();
    }
    
    // ------------------------------- HTTP ACCESS METHODS ---------------------------------------

    public string GetServerVersion() {
        return _scl.GetStringAsync("/").Result;
    }
    
    public string GetOverlayStreamAddress() {
        return "ws://" + new Uri(Cfg.BaseUrl).GetComponents(UriComponents.HostAndPort,UriFormat.UriEscaped) + "/stream/overlay";
    }
    
    public string GetEventStreamAddress() {
        return "ws://" + new Uri(Cfg.BaseUrl).GetComponents(UriComponents.HostAndPort,UriFormat.UriEscaped) + "/stream/event";
    }
    
    public string GetTimeStreamAddress() {
        return "ws://" + new Uri(Cfg.BaseUrl).GetComponents(UriComponents.HostAndPort,UriFormat.UriEscaped) + "/stream/time";
    }
    public async Task<CurrentMatch?> GetCurrentMatch() {
        string left = await _scl.GetStringAsync("/match/leftTeamId");
        string right = await _scl.GetStringAsync("/match/rightTeamId");

        if (left == "-1" || right == "-1") {
            return null;
        }

        Team leftTeam = await GetTeam(Convert.ToInt32(left));
        Team rightTeam = await GetTeam(Convert.ToInt32(right));
        return new CurrentMatch(leftTeam, rightTeam);
    }

    private async Task<Team> GetTeam(int id) {
        HttpResponseMessage data = await _scl.PutAsJsonAsync("/teams/team", new Requests.TeamRequest(id));
        return (await data.Content.ReadFromJsonAsync<Responses.TeamResponse>()).ToTeam();
    }
    
    public int GetMatchDurationSync() {
        return int.Parse(_scl.GetStringAsync("/defaultMatchLength").Result);
    }

    public async Task<int> GetMatchDuration() {
        return int.Parse(await _scl.GetStringAsync("/defaultMatchLength"));
    }

    public async Task<bool> RegisterGraphics(int internalId) {
        using HttpContent c = new StringContent(Cfg.Instances[internalId]);
        using HttpResponseMessage res = await _scl.PostAsync("/overlay/register", c);
        return await res.Content.ReadAsStringAsync() == "ok";
    }

    public async Task<GraphicsInstance> GetGraphicsInstance(string ident) {
        using HttpContent c = new StringContent(ident);
        using HttpResponseMessage res = await _scl.PutAsync("/overlay/instance", c);
        return (await JsonSerializer.DeserializeAsync<Responses.GraphicsInstanceResponse>(await res.Content.ReadAsStreamAsync())).ToGraphicsInstance();
    }

    public async Task<GraphicsInstance> GetThisGraphicsInstance(int internalId) {
        return await GetGraphicsInstance(Cfg.Instances[internalId]);
    }

    public async Task<(int leftScore, int rightScore)> GetCurrentMatchScore() {
        Responses.MatchScoreResponse r = await JsonSerializer.DeserializeAsync<Responses.MatchScoreResponse>(await _scl.GetStreamAsync("/score/matchScore"));

        return (r.left, r.right);
    }

    public async Task<bool> GetProbeModeEnabled() {
        return bool.Parse(await _scl.GetStringAsync("/overlay/probe"));
    }

    // ------------------------------- /HTTP ACCESS METHODS --------------------------------------
    
    public struct Config
    {
        public required string BaseUrl { get; init; }
        public string[] Instances { get; set; }
    }
}


public readonly struct GraphicsInstance(string ident, GraphicsMode mode) {
    public string Ident { get; } = ident;
    public GraphicsMode Mode { get; } = mode;
}

[SuppressMessage("ReSharper", "InconsistentNaming")] // because of java, again 🙄
public enum GraphicsMode {
    NONE,
    STREAM,
    TV_TWO_LEFT,
    TV_TWO_RIGHT,
    TV_THREE_LEFT,
    TV_THREE_RIGHT,
    TV_THREE_TIME
}

public readonly struct CurrentMatch(Team left, Team right) {
    public Team LeftTeam { get; } = left;
    public Team RightTeam { get; } = right;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public readonly struct Match(int id, Team left, Team right, Match.MatchStateEnum state, Match.ResultEnum result) {
    public int Id { get; } = id;
    public Team LeftTeam { get; } = left;
    public Team RightTeam { get; } = right;
    public MatchStateEnum MatchState { get; } = state;
    public ResultEnum Result { get; } = result;

    public enum MatchStateEnum {
        UPCOMING,
        PLAYING,
        ENDED
    }

    public enum ResultEnum {
        LEFT_WON,
        RIGHT_WON,
        NOT_FINISHED
    }
}

public readonly struct Team(int id, string name, Color colorBright, Color colorDark, string[] members) {
    public int Id { get; } = id;
    public string Name { get; } = name;
    public Color ColorBright { get; } = colorBright;
    public Color ColorDark { get; } = colorDark;
    public string[] Members { get; } = members;
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Requests {
    public readonly struct TeamRequest(int id) {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public int id { get; } = id;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Responses {
    public readonly struct TeamResponse {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable InconsistentNaming
        public int id { get; init; }
        public string name { get;  init; }
        public string colorBright { get;  init; }
        public string colorDark { get;  init; }
        public string[] members { get;  init; }
        // ReSharper disable InconsistentNaming
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        public Team ToTeam() => new(id, name, ParseColor(colorBright),ParseColor(colorDark),members);

        private static Color ParseColor(string hexString) {
            return new Color(
                int.Parse(hexString[..2], NumberStyles.HexNumber)/255f,
                int.Parse(hexString.Substring(2,2), NumberStyles.HexNumber)/255f,
                int.Parse(hexString.Substring(4,2), NumberStyles.HexNumber)/255f);
        }
    }
    
    public readonly struct MatchScoreResponse {
        public int left { get; init; }
        public int right { get; init; }
    }
    public readonly struct GraphicsInstanceResponse {
        public string ident { get; init; }
        public string mode { get; init; }

        public GraphicsInstance ToGraphicsInstance() {
            if(Enum.TryParse(mode, true, out GraphicsMode m))
                return new GraphicsInstance(ident,m);
            throw new IOException("Unable to parse mode");
        }
    }
}