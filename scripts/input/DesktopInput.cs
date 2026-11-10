using Godot;

/// <summary>Add desktop bindings without replacing Godot's default UI controls.</summary>
public partial class DesktopInput : Node
{
	public override void _Ready()
	{
		AddKey("ui_left", Key.A);
		AddKey("ui_right", Key.D);
		AddKey("ui_up", Key.W);
		AddKey("ui_down", Key.S);

		// Mobile touch can be emulated as a mouse click; it has its own fire control.
		if (!OS.HasFeature("android") && !OS.HasFeature("ios"))
			InputMap.ActionAddEvent("fire", new InputEventMouseButton { ButtonIndex = MouseButton.Left });
	}

	private static void AddKey(string action, Key key)
	{
		InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
	}
}
