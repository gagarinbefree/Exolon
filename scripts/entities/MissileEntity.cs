using Godot;

// Original movement uses 60 updates per second and tracks the player's top edge.
public sealed class MissileEntity
{
	public Vector2 Position = new(512, 114);
	public bool Fast { get; private set; }
	public Rect2 Bounds => new(Position, new Vector2(32, 32));
	public void Update(float playerTop)
	{
		if (!Fast && Position.X <= 280) Fast = true;
		Position.X -= Fast ? 3f : 1.5f;
		if (Position.Y > playerTop) Position.Y -= 1;
		else if (Position.Y < playerTop) Position.Y += 1;
	}
}
