using Godot;

// Default creator in L01S04: every second, spawn only while the player is left of x=320.
public sealed class BubbleCreatorEntity
{
	private float _timer;
	public string Behavior = "";
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
