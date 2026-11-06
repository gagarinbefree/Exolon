using Godot;

// Capturable launcher with two alternating random firing positions, as in melonJS.
public class DoubleLauncherEntity
{
	private int _timer;
	private int _duration;
	public int Duration => _duration;
	public int DurationMin { get; }
	public int DurationMax { get; }
	public Rect2 Bounds { get; }
	public bool Captured { get; private set; }
	public int CapturePoints { get; }
	private readonly int _lanes;
	public DoubleLauncherEntity(int x, int bottom, RandomNumberGenerator random, int width = 64, int height = 48, int capturePoints = 2000, int lanes = 2, int durationMin = 20, int durationMax = 160)
	{
		Bounds = new Rect2(x, bottom - height, width, height);
		CapturePoints = capturePoints;
		_lanes = lanes;
		DurationMin = durationMin; DurationMax = durationMax;
		ResetTimer(random);
	}
	private void ResetTimer(RandomNumberGenerator random) { _timer = 0; _duration = random.RandiRange(DurationMin, DurationMax); }
	public bool Update(float playerLeft, RandomNumberGenerator random, out Vector2 bullet)
	{
		bullet = default;
		if (++_timer <= _duration) return false;
		ResetTimer(random);
		if (playerLeft > Bounds.Position.X - 110) return false;
		bullet = Bounds.Position + new Vector2(-16, random.RandiRange(0, _lanes - 1) * 16);
		return true;
	}
	public bool Capture()
	{
		if (Captured) return false;
		Captured = true;
		return true;
	}
}
