using Godot;

// Same delayed rise, hold, and return cycle as the melonJS PistonEntity.
public sealed class PistonEntity
{
	private float _time;
	private float _wait;
	public float Rise { get; private set; }
	public PistonEntity(RandomNumberGenerator random) => _wait = random.RandfRange(1f, 5f);
	public void Update(float dt, RandomNumberGenerator random)
	{
		_time += dt;
		float t = _time - _wait;
		if (t < 0) Rise = 0;
		else if (t < 0.3f) Rise = 64 * t / 0.3f;
		else if (t < 1.3f) Rise = 64;
		else if (t < 1.6f) Rise = 64 * (1 - (t - 1.3f) / 0.3f);
		else { _time = 0; _wait = random.RandfRange(1f, 5f); Rise = 0; }
	}
}
