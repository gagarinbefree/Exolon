using Godot;

// CircularMovementBehavior used by the first jellyfish creator in melonJS.
public sealed class JellyfishEntity
{
	private static readonly int[] Frames = { 0, 1, 2, 1 };
	private readonly int _color;
	private readonly string _behavior;
	private readonly RandomNumberGenerator _random;
	private readonly CheckpointMovement? _path;
	private int _ticks;
	private int _straightTicks;
	private float _angle = Mathf.Pi;
	private int _phase;
	public Vector2 Position;
	public bool Circular => _phase == 1;
	public bool Fast => _straightTicks > 40;
	public int Frame => _color * 3 + Frames[_ticks / 3 % Frames.Length];
	public Rect2 Bounds => new(Position, new Vector2(32, 32));
	public JellyfishEntity(Vector2 playerTopLeft, RandomNumberGenerator random, string behavior = "circular")
	{
		_random = random;
		_behavior = behavior;
		Position = behavior == "up_and_down" ? new Vector2(512, playerTopLeft.Y + 2) : behavior == "swing_and_acceleration"
			? new Vector2(512 + random.RandfRange(0, 32), playerTopLeft.Y + 2 + random.RandfRange(-4, 0))
			: new Vector2(512, playerTopLeft.Y + 2 + random.RandfRange(-16, 16));
		if (behavior == "up_and_down") _path = new CheckpointMovement(false, random);
		_color = random.RandiRange(0, 5);
	}
	public void Update()
	{
		if (_path != null) { Position += _path.Step(); _ticks++; return; }
		if (_behavior == "swing_and_acceleration")
		{
			if (Position.X >= 336) Position.Y += _random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20);
			else _straightTicks++;
			Position.X -= Fast ? 4 : 1.5f;
			_ticks++;
			return;
		}
		if (_phase == 0 && Position.X < 220) _phase = 1;
		if (_phase == 1)
		{
			Position.X += Mathf.Cos(_angle) * 64 / 20;
			Position.Y += Mathf.Sin(_angle) * 64 / 18;
			_angle += 0.05f;
			if (_angle >= 3 * Mathf.Pi) _phase = 2;
		}
		else Position.X -= 3;
		_ticks++;
	}
}
