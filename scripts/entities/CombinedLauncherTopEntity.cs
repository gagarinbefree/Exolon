using Godot;

public sealed class CombinedLauncherTopEntity : TurretEntity
{
	public CombinedLauncherTopEntity(int x, int bottom) => Bounds = new Rect2(x, bottom - 32, 80, 32);
}
