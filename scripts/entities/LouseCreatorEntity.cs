using Godot;

public sealed class LouseCreatorEntity
{
	private float _timer;
	public string Behavior = "acceleration";
	public float Delay = 0.8f;
	public bool Update(float dt, Vector2 playerTopLeft, bool playerDead)
	{
		if (playerDead) _timer = 0;
		_timer += dt;
		if (_timer <= Delay) return false;
		_timer = 0;
		return playerTopLeft.X <= 320;
	}
}
