using Godot;

// The melonJS harbringer has a 128-pixel sprite but only a 32-pixel collision box.
public sealed class HarbringerEntity
{
	private int _ticks;
	public Vector2 Position;
	public Rect2 Bounds => new(Position, new Vector2(32, 32));
	public int Frame => _ticks / 3 % 26;
	public HarbringerEntity(Vector2 playerTopLeft) => Position = new Vector2(512, playerTopLeft.Y + 2);
	public void Update() { Position.X -= 3; _ticks++; }
}
