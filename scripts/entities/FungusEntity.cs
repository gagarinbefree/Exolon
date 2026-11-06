using Godot;

public sealed class FungusEntity : ObstacleEntity
{
	public FungusEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 64, 48, 64);
}
