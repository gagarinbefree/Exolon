using Godot;

// The rockets in zone 2 are stationary obstacles in the original game.
public sealed class RocketEntity : ObstacleEntity
{
	public RocketEntity(int x, int bottom) => Bounds = new Rect2(x + 12, bottom - 86, 52, 86);
}
