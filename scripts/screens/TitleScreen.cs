using Godot;
using System;

public partial class TitleScreen : Node2D
{
	private const string ReleaseDownloadRoot = "https://github.com/gagarinbefree/Exolon/releases/download/";
	private const string ApiUrl = "https://api.github.com/repos/gagarinbefree/Exolon/releases/latest";
	private AudioStreamPlayer _audio = null!;
	private HttpRequest? _request;
	private bool _updatesScreen;
	private int _selection;
	private string _status = "";
	private string _latest = "";
	private string _downloadUrl = "";
	private bool _updateAvailable;
	private bool _hasSave;

	public override void _Ready()
	{
		_hasSave = AutoSave.HasSave();
		_audio = new AudioStreamPlayer { Stream = GD.Load<AudioStream>("res://assets/sound/theme.ogg") };
		AddChild(_audio);
		_audio.Play();
		QueueRedraw();
	}

	public override void _ExitTree()
	{
		if (IsInstanceValid(_audio)) { _audio.Stop(); _audio.Stream = null; }
	}

	public override void _PhysicsProcess(double delta)
	{
		bool firePressed = Input.IsActionJustPressed("fire");
		if (!firePressed && (Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down")))
		{
			if (!_updatesScreen) _selection = (_selection + (Input.IsActionJustPressed("ui_up") ? -1 : 1) + (_hasSave ? 3 : 2)) % (_hasSave ? 3 : 2);
			else if (_updateAvailable) _selection = 1 - _selection;
			QueueRedraw();
		}
		if (!firePressed) return;
		if (!_updatesScreen)
		{
			if (_selection == 0 || (_hasSave && _selection == 1))
			{
				if (_selection == 0) AutoSave.StartNew();
				else if (!AutoSave.PrepareContinue()) { _hasSave = false; _selection = 0; QueueRedraw(); return; }
				if (OS.HasFeature("web")) JavaScriptBridge.Eval("window.exolonGameStarted?.()", true);
				GetTree().ChangeSceneToFile("res://scenes/screens/first_screen.tscn");
			}
			else StartUpdateCheck();
		}
		else if (_updateAvailable && _selection == 0) ApplyUpdate();
		else CancelUpdate();
	}

	private void StartUpdateCheck()
	{
		_updatesScreen = true;
		_updateAvailable = false;
		_selection = 1;
		_latest = "";
		_downloadUrl = "";
		_status = "CHECKING...";
		QueueRedraw();
		if (!OS.HasFeature("web") && !OS.HasFeature("android"))
		{
			_status = "NOT AVAILABLE ON DESKTOP";
			QueueRedraw();
			return;
		}
		string url = OS.HasFeature("web")
			? JavaScriptBridge.Eval("new URL('version.json', location.href).href", true).AsString()
			: ApiUrl;
		_request = new HttpRequest { Timeout = 12.0 };
		AddChild(_request);
		_request.RequestCompleted += UpdateResponse;
		string[] headers = OS.HasFeature("android") ? new[] { "User-Agent: Exolon-Godot" } : Array.Empty<string>();
		Error error = _request.Request(url + "?check=" + Time.GetUnixTimeFromSystem(), headers);
		if (error != Error.Ok) FinishWithError();
	}

	private void UpdateResponse(long result, long responseCode, string[] headers, byte[] body)
	{
		if (result != (long)HttpRequest.Result.Success || responseCode != 200)
		{
			FinishWithError();
			return;
		}
		Variant json = Json.ParseString(System.Text.Encoding.UTF8.GetString(body));
		if (json.VariantType != Variant.Type.Dictionary) { FinishWithError(); return; }
		var data = json.AsGodotDictionary();
		string key = OS.HasFeature("web") ? "version" : "tag_name";
		if (!data.ContainsKey(key)) { FinishWithError(); return; }
		_latest = data[key].AsString().TrimStart('v');
		string current = ProjectSettings.GetSetting("application/config/version", "0.0.0").AsString();
		if (!Version.TryParse(_latest, out Version? latest) || !Version.TryParse(current, out Version? installed))
		{
			FinishWithError();
			return;
		}
		_updateAvailable = latest > installed;
		if (_updateAvailable && OS.HasFeature("android"))
		{
			_downloadUrl = AndroidApkUrl(data, _latest);
			if (_downloadUrl == "")
			{
				_updateAvailable = false;
				_status = "UPDATE FILE NOT FOUND";
				_selection = 1;
				ClearRequest();
				QueueRedraw();
				return;
			}
		}
		_status = _updateAvailable ? "UPDATE AVAILABLE" : "YOU ARE UP TO DATE";
		_selection = _updateAvailable ? 0 : 1;
		ClearRequest();
		QueueRedraw();
	}

	private void FinishWithError()
	{
		_status = "CHECK FAILED - TRY AGAIN";
		ClearRequest();
		QueueRedraw();
	}

	private void ClearRequest()
	{
		if (IsInstanceValid(_request)) { _request!.CancelRequest(); _request.QueueFree(); }
		_request = null;
	}

	private void CancelUpdate()
	{
		ClearRequest();
		_updatesScreen = false;
		_selection = _hasSave ? 2 : 1;
		QueueRedraw();
	}

	private void ApplyUpdate()
	{
		if (OS.HasFeature("web")) JavaScriptBridge.Eval("location.reload()", true);
		else if (OS.ShellOpen(_downloadUrl) != Error.Ok)
		{
			_status = "DOWNLOAD FAILED";
			QueueRedraw();
		}
	}

	// Only open the APK belonging to the version reported by our release.
	private static string AndroidApkUrl(Godot.Collections.Dictionary release, string version)
	{
		if (!Version.TryParse(version, out Version? parsed) || parsed.Build < 0 || parsed.Revision >= 0 ||
			version != parsed.ToString(3) || !release.ContainsKey("tag_name") ||
			release["tag_name"].AsString() != "v" + version ||
			!release.ContainsKey("assets") || release["assets"].VariantType != Variant.Type.Array) return "";
		string name = $"Exolon-{version}.apk";
		string expectedUrl = $"{ReleaseDownloadRoot}v{version}/{name}";
		foreach (Variant item in release["assets"].AsGodotArray())
		{
			if (item.VariantType != Variant.Type.Dictionary) continue;
			var asset = item.AsGodotDictionary();
			if (asset.ContainsKey("name") && asset.ContainsKey("browser_download_url") &&
				asset["name"].AsString() == name && asset["browser_download_url"].AsString() == expectedUrl)
				return expectedUrl;
		}
		return "";
	}

	public string TestAndroidApkUrl(string releaseJson, string version)
	{
		Variant release = Json.ParseString(releaseJson);
		return release.VariantType == Variant.Type.Dictionary ? AndroidApkUrl(release.AsGodotDictionary(), version) : "";
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/title.png"), new Vector2(128, 0));
		DrawText("green", "BY  RAFFAELE CECCO", 112, 96);
		if (!_updatesScreen)
		{
			DrawOption("PLAY", _hasSave ? 168 : 184, _selection == 0);
			if (_hasSave) DrawOption("CONTINUE", 208, _selection == 1);
			DrawOption("CHECK FOR UPDATES", _hasSave ? 248 : 224, _selection == (_hasSave ? 2 : 1));
			DrawText("green", "UP DOWN + FIRE", 144, 288);
		}
		else
		{
			DrawCentered("cyan", "CHECK FOR UPDATES", 152);
			DrawCentered("green", _status, 192);
			if (_updateAvailable)
			{
				DrawCentered("green", "NEW VERSION " + _latest, 216);
				DrawOption("UPDATE", 256, _selection == 0);
			}
			DrawOption("CANCEL", 296, _selection == 1);
		}
		DrawText("red", "EXOLON COPYRIGHT 1987 HEWSON", 32, 368);
	}

	private void DrawCentered(string color, string text, int y) => DrawText(color, text, (512 - text.Length * 16) / 2, y);

	private void DrawOption(string text, int y, bool selected)
	{
		int x = (512 - text.Length * 16) / 2;
		if (selected)
			DrawTextureRectRegion(GD.Load<Texture2D>("res://assets/images/egg.png"), new Rect2(x - 24, y, 16, 16), new Rect2(0, 0, 16, 16));
		DrawText(selected ? "purple" : "cyan", text, x, y);
	}

	private void DrawText(string color, string text, int x, int y)
	{
		Texture2D font = GD.Load<Texture2D>($"res://assets/images/font_{color}.png");
		int columns = font.GetWidth() / 16;
		foreach (char c in text)
		{
			int index = c - 32;
			DrawTextureRectRegion(font, new Rect2(x, y, 16, 16), new Rect2(index % columns * 16, index / columns * 16, 16, 16));
			x += 16;
		}
	}
}
