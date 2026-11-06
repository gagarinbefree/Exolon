using Godot;

// The original beam is solid and lethal, and needs 50 blaster hits.
public sealed class BeamEntity : ObstacleEntity
{
	public int HitPoints = 50;
	public BeamEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 256, 16, 256);
}
