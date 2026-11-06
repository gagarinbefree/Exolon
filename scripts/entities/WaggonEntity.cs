using Godot;

// Waggons use the standard grenade-destructible obstacle behavior.
public sealed class WaggonEntity : ObstacleEntity
{
	public WaggonEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 80, 64, 80);
}
