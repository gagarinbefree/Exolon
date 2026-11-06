using Godot;

public sealed class JellyfishCreatorEntity
{
	private float _timer;
	public string Behavior = "circular";
	public float Delay = 1f;
	public bool Update(float dt, Vector2 playerTopLeft, bool playerDead)
	{
		if (playerDead) _timer = 0;
		_timer += dt;
		if (_timer <= Delay) return false;
		_timer = 0;
		return playerTopLeft.X <= 320;
	}
}
