using System;

// melonJS awards 25 points after each 0.04-second interval and flushes the
// remainder when leaving the level.
public sealed class AwardPointsEntity
{
	private float _timer;
	private readonly int _step;
	public int Remaining { get; private set; }
	public AwardPointsEntity(int points, int step = 25) { Remaining = points; _step = step; }
	public int Update(float dt)
	{
		_timer += dt;
		if (_timer <= 0.04f || Remaining == 0) return 0;
		_timer = 0;
		int increment = Math.Min(_step, Remaining);
		Remaining -= increment;
		return increment;
	}
}
