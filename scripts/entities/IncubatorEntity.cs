using Godot;

// The incubator is an ordinary grenade-destructible obstacle containing eight eggs.
public sealed class IncubatorEntity : ObstacleEntity
{
	public IncubatorEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 96, 64, 96);
	public Rect2 EggBounds => new(Bounds.Position + new Vector2(16, 16), new Vector2(32, 32));
}
