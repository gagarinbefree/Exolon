using Godot;

public sealed class CombinedLauncherBottomEntity : DoubleLauncherEntity
{
	public CombinedLauncherBottomEntity(int x, int bottom, RandomNumberGenerator random) : base(x, bottom, random, 96, 32, 3000, 1) { }
}
