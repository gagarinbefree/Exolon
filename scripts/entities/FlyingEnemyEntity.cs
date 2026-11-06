using Godot;

// Shared flying enemy body for Fir and Flasher; movement matches melonJS behaviors.
public sealed class FlyingEnemyEntity
{
	private readonly string _behavior;
	private readonly CheckpointMovement? _path;
	private readonly int _color;
	private int _ticks, _straightTicks;
	public string Kind { get; }
	public Vector2 Position;
	public Rect2 Bounds => new(Position, new Vector2(32, 32));
	public int Phase => _path?.Phase ?? 0;
	public bool Fast => _behavior == "acceleration" ? Position.X < 304 : _straightTicks > 40;
	public int Frame => Kind == "fir" ? _color : _color * 2 + _ticks / 4 % 2;

	public FlyingEnemyEntity(string kind, string behavior, Vector2 playerTopLeft, RandomNumberGenerator random)
	{
		Kind = kind;
		_behavior = behavior;
		Position = new Vector2(512, playerTopLeft.Y + 2 + (kind == "fir" ? 8 : 0));
		_color = random.RandiRange(0, 5);
		if (behavior == "acceleration") Position.Y += random.RandfRange(-32, 32);
		else if (behavior == "swing") { Position.X += random.RandfRange(0, 32); Position.Y += random.RandfRange(-16, 0); }
		else if (behavior == "swing_and_acceleration") { Position.X += random.RandfRange(0, 32); Position.Y += random.RandfRange(-4, 0); }
		else _path = new CheckpointMovement(kind == "fir", random);
	}

	public void Update(RandomNumberGenerator random)
	{
		if (_path != null) Position += _path.Step();
		else if (_behavior == "acceleration") Position.X -= Fast ? 4 : 1.5f;
		else if (_behavior == "swing") { Position.X -= 1.7f; Position.Y += random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20); }
		else
		{
			if (Position.X >= 336) Position.Y += random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20);
			else _straightTicks++;
			Position.X -= Fast ? 4 : 1.5f;
		}
		_ticks++;
	}
}
