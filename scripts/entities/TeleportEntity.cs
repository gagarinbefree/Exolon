using Godot;

// A teleport is on a later TMX object layer than the player in zone 3.
[Tool]
public partial class TeleportEntity : Node2D
{
	private Texture2D _texture = null!;
	private int _ticks;
	public override void _Ready()
	{
		_texture = GD.Load<Texture2D>("res://assets/images/teleport.png");
		if (Engine.IsEditorHint()) SetPhysicsProcess(false);
	}
	public override void _PhysicsProcess(double delta) { _ticks++; QueueRedraw(); }
	public override void _Draw() => DrawTextureRectRegion(_texture, new Rect2(0, 0, 64, 96), new Rect2((_ticks / 3 % 8) * 64, 0, 64, 96));
}
