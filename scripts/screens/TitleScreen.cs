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
	private SaveMenuOptions _options = null!;
	private Node2D _updateMenu = null!;
	private BitmapText _instructions = null!, _statusLabel = null!, _versionLabel = null!;
	private MenuOption _update = null!, _cancel = null!;

	public override void _Ready()
	{
		_options = GetNode<SaveMenuOptions>("Options");
		_instructions = GetNode<BitmapText>("Instructions");
		_updateMenu = GetNode<Node2D>("UpdateMenu");
		_statusLabel = _updateMenu.GetNode<BitmapText>("Status");
		_versionLabel = _updateMenu.GetNode<BitmapText>("Version");
		_update = _updateMenu.GetNode<MenuOption>("Update");
		_cancel = _updateMenu.GetNode<MenuOption>("Cancel");
		_hasSave = AutoSave.HasSave();
		_audio = new AudioStreamPlayer { Stream = GD.Load<AudioStream>("res://assets/sound/theme.ogg") };
		AddChild(_audio);
		_audio.Play();
		RefreshMenu();
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
			RefreshMenu();
		}
		if (!firePressed) return;
		if (!_updatesScreen)
		{
			if (_selection == 0 || (_hasSave && _selection == 1))
			{
				if (_hasSave && _selection == 0)
				{
					if (!AutoSave.PrepareContinue()) { _hasSave = false; _selection = 0; RefreshMenu(); return; }
				}
				else AutoSave.StartNew();
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
		RefreshMenu();
		if (!OS.HasFeature("web") && !OS.HasFeature("android"))
		{
			_status = "NOT AVAILABLE ON DESKTOP";
			RefreshMenu();
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
				RefreshMenu();
				return;
			}
		}
		_status = _updateAvailable ? "UPDATE AVAILABLE" : "YOU ARE UP TO DATE";
		_selection = _updateAvailable ? 0 : 1;
		ClearRequest();
		RefreshMenu();
	}

	private void FinishWithError()
	{
		_status = "CHECK FAILED - TRY AGAIN";
		ClearRequest();
		RefreshMenu();
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
		RefreshMenu();
	}

	private void ApplyUpdate()
	{
		if (OS.HasFeature("web")) JavaScriptBridge.Eval("location.reload()", true);
		else if (OS.ShellOpen(_downloadUrl) != Error.Ok)
		{
			_status = "DOWNLOAD FAILED";
			RefreshMenu();
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

	private void RefreshMenu()
	{
		_options.Visible = _instructions.Visible = !_updatesScreen;
		_updateMenu.Visible = _updatesScreen;
		if (!_updatesScreen)
		{
			_options.ShowSelection(_hasSave, _selection);
			return;
		}
		_statusLabel.Text = _status;
		_versionLabel.Visible = _update.Visible = _updateAvailable;
		_versionLabel.Text = "NEW VERSION " + _latest;
		_update.Selected = _selection == 0;
		_cancel.Selected = _selection == 1;
	}
}
