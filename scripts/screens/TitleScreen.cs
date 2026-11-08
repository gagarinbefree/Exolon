using Godot;
using System;

public partial class TitleScreen : Node2D
{
	private const string ReleaseUrl = "https://github.com/gagarinbefree/Exolon/releases/latest";
	private const string ApiUrl = "https://api.github.com/repos/gagarinbefree/Exolon/releases/latest";
	private AudioStreamPlayer _audio = null!;
	private HttpRequest? _request;
	private bool _updatesScreen;
	private int _selection;
	private string _status = "";
	private string _latest = "";
	private bool _updateAvailable;

	public override void _Ready()
	{
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
			if (!_updatesScreen || _updateAvailable) _selection = 1 - _selection;
			QueueRedraw();
		}
		if (!firePressed) return;
		if (!_updatesScreen)
		{
			if (_selection == 0)
			{
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
		_selection = 1;
		QueueRedraw();
	}

	private static void ApplyUpdate()
	{
		if (OS.HasFeature("web")) JavaScriptBridge.Eval("location.reload()", true);
		else OS.ShellOpen(ReleaseUrl);
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/title.png"), new Vector2(128, 0));
		DrawText("green", "BY  RAFFAELE CECCO", 112, 96);
		if (!_updatesScreen)
		{
			DrawOption("PLAY", 184, _selection == 0);
			DrawOption("CHECK FOR UPDATES", 224, _selection == 1);
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
