using Godot;

// RadarEntity inherits the solid, grenade and score behavior of ObstacleEntity,
// just as in melonJS; its collision box is inset six pixels horizontally.
public sealed class RadarEntity : ObstacleEntity
{
	public RadarEntity(int x, int bottom) => Bounds = new Rect2(x + 6, bottom - 128, 68, 128);
}
