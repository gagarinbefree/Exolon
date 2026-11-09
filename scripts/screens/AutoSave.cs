using Godot;
using System;

// A checkpoint is the state at the moment a new screen is entered.
public static class AutoSave
{
	public sealed record Checkpoint(string Level, int Zone, int Ammo, int Grenades, int Lives, int Points, bool Armored, float Y);
	private static Checkpoint? _pending;
	private static string Path => string.IsNullOrEmpty(OS.GetEnvironment("EXOLON_SAVE_PATH"))
		? "user://autosave.cfg" : OS.GetEnvironment("EXOLON_SAVE_PATH");

	public static bool HasSave() => Load() != null;
	public static bool PrepareContinue()
	{
		_pending = Load();
		return _pending != null;
	}
	public static void StartNew() => _pending = null;
	public static Checkpoint? Consume()
	{
		Checkpoint? saved = _pending;
		_pending = null;
		return saved;
	}

	public static void Save(Checkpoint data)
	{
		var file = new ConfigFile();
		file.SetValue("checkpoint", "version", 1);
		file.SetValue("checkpoint", "level", data.Level);
		file.SetValue("checkpoint", "zone", data.Zone);
		file.SetValue("checkpoint", "ammo", data.Ammo);
		file.SetValue("checkpoint", "grenades", data.Grenades);
		file.SetValue("checkpoint", "lives", data.Lives);
		file.SetValue("checkpoint", "points", data.Points);
		file.SetValue("checkpoint", "armored", data.Armored);
		file.SetValue("checkpoint", "y", data.Y);
		string temporary = Path + ".tmp";
		Error error = file.Save(temporary);
		if (error == Error.Ok)
			error = DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(temporary), ProjectSettings.GlobalizePath(Path));
		if (error != Error.Ok) { GD.PushWarning($"Could not write autosave: {error}"); return; }
		if (OS.HasFeature("web")) JavaScriptBridge.ForceFsSync();
	}

	public static Checkpoint? Load()
	{
		var file = new ConfigFile();
		if (file.Load(Path) != Error.Ok) return null;
		try
		{
			if (file.GetValue("checkpoint", "version", 0).AsInt32() != 1) return null;
			string level = file.GetValue("checkpoint", "level", "").AsString();
			int zone = file.GetValue("checkpoint", "zone", -1).AsInt32();
			int ammo = file.GetValue("checkpoint", "ammo", -1).AsInt32();
			int grenades = file.GetValue("checkpoint", "grenades", -1).AsInt32();
			int lives = file.GetValue("checkpoint", "lives", -1).AsInt32();
			int points = file.GetValue("checkpoint", "points", -1).AsInt32();
			bool armored = file.GetValue("checkpoint", "armored", false).AsBool();
			float y = file.GetValue("checkpoint", "y", -1).AsSingle();
			if (!LevelMap.CanEnter(level) || !LevelMap.Exists(level) || zone < 0 || zone > 74 ||
				ammo < 0 || ammo > 99 || grenades < 0 || grenades > 10 || lives < 1 || lives > 9 ||
				points < 0 || !float.IsFinite(y) || y < 0 || y > 384) return null;
			int expectedZone = (int.Parse(level.AsSpan(1, 2)) - 1) * 25 + int.Parse(level.AsSpan(4, 2)) - 1;
			return zone == expectedZone ? new Checkpoint(level, zone, ammo, grenades, lives, points, armored, y) : null;
		}
		catch (Exception error) when (error is InvalidCastException or FormatException or OverflowException)
		{
			GD.PushWarning($"Ignoring invalid autosave: {error.Message}");
			return null;
		}
	}
}
