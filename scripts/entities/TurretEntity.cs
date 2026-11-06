using Godot;

// The original turret owns a separate tube with its own shot timer and animation.
public class TurretEntity : ObstacleEntity
{
	private int _timer;
	private int _duration;
	private int _fireTicks = -1;
	public int TubeFrame => _fireTicks < 0 ? 0 : Mathf.Max(0, 7 - _fireTicks / 3);

	public void ResetTimer(RandomNumberGenerator random)
	{
		_timer = 0;
		_duration = random.RandiRange(50, 300);
	}

	public bool Update(RandomNumberGenerator random)
	{
		if (_fireTicks >= 0 && ++_fireTicks >= 24) _fireTicks = -1;
		if (++_timer <= _duration) return false;
		ResetTimer(random);
		_fireTicks = 0;
		return true;
	}
}
