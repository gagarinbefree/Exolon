using Godot;

public sealed class FlyingCreatorEntity
{
	private float _timer;
	public readonly string Kind;
	public readonly string Behavior;
	public readonly float Delay;
	public FlyingCreatorEntity(string kind, string behavior, float delay)
	{
		Kind = kind; Behavior = behavior; Delay = delay > 0 ? delay : kind == "fir" ? 1.2f : 0.8f;
	}
	public bool Update(float dt, Vector2 playerTopLeft, bool playerDead)
	{
		if (playerDead) _timer = 0;
		_timer += dt;
		if (_timer <= Delay) return false;
		_timer = 0;
		return playerTopLeft.X <= 320;
	}
}
