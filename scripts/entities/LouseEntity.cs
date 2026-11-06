using Godot;

// AccelerationMovementBehavior used by L02S02.
public sealed class LouseEntity
{
	private static readonly int[] Frames = { 0, 1, 2, 1 };
	private readonly int _color;
	private int _ticks;
	private readonly string _behavior;
	private readonly RandomNumberGenerator _random;
	private float _angle = Mathf.Pi;
	private int _phase;
	public Vector2 Position;
	public bool Fast { get; private set; }
	public bool Circular => _phase == 1;
	public Rect2 Bounds => new(Position, new Vector2(32, 32));
	public int Frame => _color * 3 + Frames[_ticks / 3 % Frames.Length];
	public LouseEntity(Vector2 playerTopLeft, RandomNumberGenerator random, string behavior = "acceleration")
	{
		_behavior = behavior;
		_random = random;
		Position = behavior == "swing"
			? new Vector2(512 + random.RandfRange(0, 32), playerTopLeft.Y + 2 + random.RandfRange(-16, 0))
			: new Vector2(512, playerTopLeft.Y + 2 + random.RandfRange(behavior == "circular" ? -16 : -32, behavior == "circular" ? 16 : 32));
		_color = random.RandiRange(0, 5);
	}
	public void Update()
	{
		if (_behavior == "swing")
		{
			Position.X -= 1.7f;
			Position.Y += _random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20);
		}
		else if (_behavior == "circular")
		{
			if (_phase == 0 && Position.X < 220) _phase = 1;
			if (_phase == 1)
			{
				Position.X += Mathf.Cos(_angle) * 64 / 20;
				Position.Y += Mathf.Sin(_angle) * 64 / 18;
				_angle += 0.05f;
				if (_angle >= 3 * Mathf.Pi) _phase = 2;
			}
			else Position.X -= 3;
		}
		else
		{
			if (Position.X < 304) Fast = true;
			Position.X -= Fast ? 4 : 1.5f;
		}
		_ticks++;
	}
}
