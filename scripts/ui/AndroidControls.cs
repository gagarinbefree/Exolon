using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>Android-only controls, matching the web layout without affecting the web or desktop.</summary>
public partial class AndroidControls : CanvasLayer
{
	private Control _overlay = null!;
	private readonly Dictionary<int, string[]> _pointers = new();
	private readonly Dictionary<string, int> _counts = new();
	private readonly List<(Rect2 Rect, string[] Actions, string Label)> _zones = new();
	private bool _enabled;

	public override void _Ready()
	{
		if (OS.HasFeature("android"))
		{
			DisplayServer.ScreenSetOrientation(DisplayServer.ScreenOrientation.Sensor);
			Enable();
		}
	}

	// Used by the Godot interaction test on a desktop runner.
	public void TestEnable() => Enable();

	private void Enable()
	{
		if (_enabled) return;
		_enabled = true;
		Layer = 100;
		RenderingServer.SetDefaultClearColor(Colors.Black);
		GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
		_overlay = new TouchOverlay { OwnerControls = this, MouseFilter = Control.MouseFilterEnum.Ignore };
		AddChild(_overlay);
		_overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		GetViewport().SizeChanged += UpdateLayout;
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		ReleaseAll();
		Vector2 size = GetViewport().GetVisibleRect().Size;
		if (size.X <= 0 || size.Y <= 0) return;
		bool landscape = size.X > size.Y;
		// Reserve an accessible control strip and the gap on each side.
		float screenScale = landscape
			? (OS.HasFeature("android") ? Mathf.Max(.5f, DisplayServer.ScreenGetScale()) : 1f)
			: Mathf.Min(size.X, size.Y) / 390f;
		float gap = 20f * screenScale;
		bool tabletLayout = landscape && size.X / size.Y <= 1.6f;
		float gameMinimum = Mathf.Min(256f * screenScale, Mathf.Max(0, (size.X - 2f * gap) / 3f));
		float controlMinimum = Mathf.Min((tabletLayout ? 400f : 160f) * screenScale, Mathf.Max(0, size.X - 2f * gap - gameMinimum));
		float availableGameWidth = Mathf.Max(0, size.X - controlMinimum - 2f * gap);
		float gameWidth = landscape ? Mathf.Min(availableGameWidth, size.Y * 4f / 3f) : size.X;
		float scale = gameWidth / 512f;
		Vector2 gameSize = new(gameWidth, 384f * scale);
		float controlsWidth = Mathf.Max(0, size.X - gameWidth - 2f * gap);
		float fireWidth = controlsWidth * .3f;
		Vector2 gameTopLeft = landscape
			? new Vector2(fireWidth + gap, (size.Y - gameSize.Y) / 2f)
			: (size - gameSize) / 2f;
		GetViewport().CanvasTransform = new Transform2D(0, new Vector2(scale, scale), 0, gameTopLeft);

		Rect2 fire, directions;
		if (landscape)
		{
			float directionsWidth = Mathf.Max(0, size.X - gameTopLeft.X - gameSize.X - gap);
			fire = new Rect2(0, 0, fireWidth, size.Y);
			directions = new Rect2(size.X - directionsWidth, 0, directionsWidth, size.Y);
		}
		else
		{
			float top = Mathf.Min(size.Y, gameTopLeft.Y + gameSize.Y + gap);
			fire = new Rect2(0, top, size.X * .293f, size.Y - top);
			directions = new Rect2(size.X * .293f, top, size.X * .707f, size.Y - top);
		}
		_zones.Clear();
		AddZone(fire, "FIRE", "fire");
		float x = directions.Position.X, y = directions.Position.Y;
		float w = directions.Size.X, h = directions.Size.Y;
		float l = w * 9f / 27f, c = w * 7f / 27f, r = w * 11f / 27f;
		AddZone(new Rect2(x, y, l, h * .4f), "↖", "ui_up", "ui_left");
		AddZone(new Rect2(x + l, y, c, h * .5f), "↑", "ui_up");
		AddZone(new Rect2(x + l + c, y, r, h * .4f), "↗", "ui_up", "ui_right");
		AddZone(new Rect2(x, y + h * .4f, l, h * .6f), "←", "ui_left");
		AddZone(new Rect2(x + l, y + h * .5f, c, h * .5f), "↓", "ui_down");
		AddZone(new Rect2(x + l + c, y + h * .4f, r, h * .6f), "→", "ui_right");
		_overlay.QueueRedraw();
	}

	private void AddZone(Rect2 rect, string label, params string[] actions) => _zones.Add((rect, actions, label));

	public override void _Input(InputEvent @event)
	{
		if (!_enabled) return;
		if (@event is InputEventScreenTouch touch)
			UpdatePointer(touch.Index, touch.Pressed ? touch.Position : null);
		else if (@event is InputEventScreenDrag drag)
			UpdatePointer(drag.Index, drag.Position);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationApplicationFocusOut || what == NotificationApplicationPaused) ReleaseAll();
	}

	private void UpdatePointer(int index, Vector2? position)
	{
		string[] before = _pointers.GetValueOrDefault(index) ?? System.Array.Empty<string>();
		string[] after = position is Vector2 p ? _zones.Find(zone => zone.Rect.HasPoint(p)).Actions ?? System.Array.Empty<string>() : System.Array.Empty<string>();
		foreach (string action in before.Except(after))
		{
			int count = _counts[action] - 1;
			_counts[action] = count;
			if (count == 0) Input.ActionRelease(action);
		}
		foreach (string action in after.Except(before))
		{
			int count = _counts.GetValueOrDefault(action);
			_counts[action] = count + 1;
			if (count == 0) Input.ActionPress(action);
		}
		if (after.Length == 0) _pointers.Remove(index); else _pointers[index] = after;
	}

	private void ReleaseAll()
	{
		foreach (string action in _counts.Keys) Input.ActionRelease(action);
		_counts.Clear(); _pointers.Clear();
	}

	private partial class TouchOverlay : Control
	{
		public AndroidControls OwnerControls = null!;
		public override void _Draw()
		{
			Color background = new(1, 1, 1, .02f), label = new(1, 1, 1, .4f), line = new(1, 1, 1, .13f);
			float screenScale = Mathf.Min(Size.X, Size.Y) / 390f;
			float iconSize = Mathf.Clamp(Size.X * .07f, 30f * screenScale, 56f * screenScale);
			foreach (var zone in OwnerControls._zones)
			{
				DrawRect(zone.Rect, background);
				float fittedSize = Mathf.Min(iconSize, Mathf.Min(zone.Rect.Size.X, zone.Rect.Size.Y) * .8f);
				if (fittedSize > 0) DrawIcon(zone.Rect.GetCenter(), fittedSize, zone.Label, label);
			}
			// Draw only internal separators, never the exterior border.
			for (int i = 1; i < OwnerControls._zones.Count; i++)
			{
				Rect2 rect = OwnerControls._zones[i].Rect;
				if (i is 1 or 2 or 4 or 5) DrawLine(new Vector2(rect.End.X, rect.Position.Y), rect.End, line);
				if (i is 1 or 2 or 3) DrawLine(new Vector2(rect.Position.X, rect.End.Y), rect.End, line);
			}
			if (Size.Y > Size.X && OwnerControls._zones.Count > 1)
			{
				Rect2 fire = OwnerControls._zones[0].Rect;
				DrawLine(new Vector2(fire.End.X, fire.Position.Y), fire.End, line);
			}
		}

		// Lucide/Feather crosshair and arrow-up, rotated for each direction.
		private void DrawIcon(Vector2 center, float size, string icon, Color color)
		{
			float angle = icon switch { "↗" => Mathf.Pi / 4, "→" => Mathf.Pi / 2, "↓" => Mathf.Pi,
				"←" => -Mathf.Pi / 2, "↖" => -Mathf.Pi / 4, _ => 0 };
			float scale = size / 24f;
			DrawSetTransform(center, angle, new Vector2(scale, scale));
			if (icon == "FIRE")
			{
				DrawArc(Vector2.Zero, 8, 0, Mathf.Tau, 48, color, 2.5f, true);
				DrawLine(new Vector2(0, -11), new Vector2(0, -5), color, 2.5f, true);
				DrawLine(new Vector2(0, 5), new Vector2(0, 11), color, 2.5f, true);
				DrawLine(new Vector2(-11, 0), new Vector2(-5, 0), color, 2.5f, true);
				DrawLine(new Vector2(5, 0), new Vector2(11, 0), color, 2.5f, true);
			}
			else
			{
				DrawLine(new Vector2(0, 7), new Vector2(0, -7), color, 2.5f, true);
				DrawLine(new Vector2(-7, 0), new Vector2(0, -7), color, 2.5f, true);
				DrawLine(new Vector2(0, -7), new Vector2(7, 0), color, 2.5f, true);
			}
			DrawSetTransform(Vector2.Zero);
		}
	}
}
