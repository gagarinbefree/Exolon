using Godot;

// Shared solid and grenade-destruction behavior from melonJS ObstacleEntity.
public class ObstacleEntity
{
	public string Name = "";
	public Rect2 Bounds;
	public Rect2 TubeBounds;
	public StaticBody2D Body = null!;
	public bool Alive = true;
	public int MapX;
	public int MapBottom;
	public bool Intersects(Rect2 rect) => rect.Intersects(Bounds) || (TubeBounds.HasArea() && rect.Intersects(TubeBounds));
}
