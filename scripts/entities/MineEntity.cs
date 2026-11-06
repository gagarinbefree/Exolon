using Godot;

// MineEntity in melonJS has a 4x1 collision area just above the sprite.
public sealed class MineEntity
{
	public Vector2 Position { get; }
	public bool Triggered { get; private set; }
	public Rect2 TriggerBounds => new(Position + new Vector2(14, -2), new Vector2(4, 1));
	public MineEntity(int x, int bottom) => Position = new Vector2(x, bottom - 16);
	public void Trigger() => Triggered = true;
}
