using Godot;

public partial class TeleportFlashEntity : Node2D
{
	private Texture2D _texture = null!;
	private int _ticks;
	public override void _Ready() => _texture = GD.Load<Texture2D>("res://assets/images/teleport_flash.png");
	public override void _PhysicsProcess(double delta)
	{
		if (++_ticks >= 82) { QueueFree(); return; }
		QueueRedraw();
	}
	public override void _Draw() => DrawTextureRectRegion(_texture, new Rect2(0, 0, 32, 48), new Rect2((_ticks / 2) * 32, 0, 32, 48));
}
