using Godot;

// The guidance unit is grenade-destructible and owns at most one missile.
public sealed class MissileGuidanceEntity : ObstacleEntity
{
	public MissileGuidanceEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 64, 64, 64);
}
