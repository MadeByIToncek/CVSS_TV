using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Packets;
using ArtNet.Sockets;
using CVSS_TV.API;
using Godot;

namespace CVSS_TV.DMX;

public class DmxMaster {
	private readonly ApiHandler _api;
	private readonly WebsocketHandler _wsh;
	private readonly Control _timer;
	private static ReaderWriterLock ConfigLock = new();

	private readonly ArtNetSocket _artnet;
	private Color _leftColor;
	private Color _rightColor;

	public DmxMaster(ApiHandler api, WebsocketHandler wsh, Control timer) {
		_api = api;
		_wsh = wsh;
		_timer = timer;
		_wsh.EventReceived += async (_, e) => {
			await Task.Delay(100);
			switch (e) {
				case Event.MATCH_ARM:
					await ArmLights();
					break;
				case Event.MATCH_RESET:
					await DisarmLights();
					break;
				case Event.MATCH_START:
					StartLights();
					break;
				case Event.MATCH_RECYCLE:
					RecycleLights();
					break;
				case Event.MATCH_END:
					EndLights();
					break;
				default:
					GD.Print($"Received {e}");
					break;
			}
		};
		_artnet = new ArtNetSocket
		{
			EnableBroadcast = true
		};
	}

	public async Task _Init() {
		DmxConfig dmxcfg = await ReadConfig();
		
		_artnet.Begin(IPAddress.Parse(dmxcfg.ArtnetIp),IPAddress.Parse("255.255.255.0"));
	}

	private static async Task<DmxConfig> ReadConfig() {
		try {
			if (!File.Exists("dmx.config")) {
				await GenerateConfigFile();
			}

			ConfigLock.AcquireReaderLock(TimeSpan.FromSeconds(10));
			await using FileStream fs = File.OpenRead("dmx.config");
			return await JsonSerializer.DeserializeAsync<DmxConfig>(fs);
		}
		finally {
			ConfigLock.ReleaseReaderLock();
		}
	}

	private static async Task GenerateConfigFile() {
		ConfigLock.AcquireWriterLock(TimeSpan.FromSeconds(10));
		DmxConfig cfg = new();
		await using var fs = File.Create("dmx.config");
		await JsonSerializer.SerializeAsync(fs, cfg);
		ConfigLock.ReleaseWriterLock();
	}

	private void RecycleLights() {
	}

	private void StartLights() {
	}

	private void EndLights() {
	}

	private async Task DisarmLights() {
		DmxConfig cfg = await ReadConfig();
		Tween t = _timer.GetTree().CreateTween();
		t.SetTrans(Tween.TransitionType.Cubic).TweenMethod(Callable.From<double>(d => FadeToBlack(d, cfg)), 1d, 0d, 2);
	}

	private async Task ArmLights() {
		CurrentMatch? m = await _api.GetCurrentMatch();
		DmxConfig cfg = await ReadConfig();
		Tween t = _timer.GetTree().CreateTween();
		t.SetTrans(Tween.TransitionType.Cubic).TweenMethod(Callable.From<double>(d => ExecuteFade(d, cfg,m.Value)), 0d, 1d, 2);
		_leftColor = m.Value.LeftTeam.ColorBright;
		_rightColor = m.Value.RightTeam.ColorBright;
	}

	private void FadeToBlack(double v, DmxConfig cfg) {
		ArtDmx dmxPacket = new() {
			ProtVerHi = 6,
			ProtVerLo = 9,
			Sequence = 0x00,
			Physical = 0,
			SubUni = 0,
			Net = 0,
			LengthHi = 0,
			LengthLo = 0,
			Data = new byte[512],
			Length = 512,
			Universe = 0
		};
		
		foreach (RgbLight light in cfg.RgbLights) {
			Color c = light.LeftSide ? _leftColor: _rightColor;
			dmxPacket.Data[light.R] = (byte)(c.R8*v);
			dmxPacket.Data[light.G] = (byte)(c.G8*v);
			dmxPacket.Data[light.B] = (byte)(c.B8*v);
		}
		_artnet.Send(dmxPacket);
	}

	private void ExecuteFade(double v, DmxConfig cfg, CurrentMatch m) {
		ArtDmx dmxPacket = new() {
			ProtVerHi = 6,
			ProtVerLo = 9,
			Sequence = 0x00,
			Physical = 0,
			SubUni = 0,
			Net = 0,
			LengthHi = 0,
			LengthLo = 0,
			Data = new byte[512],
			Length = 512,
			Universe = 0
		};
		
		foreach (RgbLight light in cfg.RgbLights) {
			Color c = light.LeftSide ? m.LeftTeam.ColorBright : m.RightTeam.ColorBright;
			dmxPacket.Data[light.R] = (byte)(c.R8*v);
			dmxPacket.Data[light.G] = (byte)(c.G8*v);
			dmxPacket.Data[light.B] = (byte)(c.B8*v);
		}
		_artnet.Send(dmxPacket);
	}
}

public struct DmxConfig() {
	public string ArtnetIp { get; set; } = IPAddress.Any.ToString();
	public List<RgbLight> RgbLights { get; set; } = [];
}

public struct RgbLight {
	public string Ident { get; set; }
	public byte R { get; set; }
	public byte G { get; set; }
	public byte B { get; set; }
	public bool LeftSide { get; set; }
}