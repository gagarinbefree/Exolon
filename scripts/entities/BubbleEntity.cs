using Godot;

// Default SwingMovementBehavior from the melonJS bubble enemy.
public sealed class BubbleEntity
{
	public Vector2 Position;
	public readonly int Color;
	private int _ticks;
	private readonly string _behavior;
	private readonly CheckpointMovement? _path;
	private int _phase;
	private float _angle = Mathf.Pi;
	public bool Circular => _phase == 1;
	public int Frame => Color * 3 + (_ticks / 3 % 3);
	public Rect2 Bounds => new(Position, new Vector2(32, 32));

	public BubbleEntity(Vector2 playerTopLeft, RandomNumberGenerator random, string behavior = "")
	{
		_behavior = behavior;
		Position = behavior == "zig_zag" ? new Vector2(512, playerTopLeft.Y + 2) : behavior == "circular"
			? new Vector2(512, playerTopLeft.Y + 2 + random.RandfRange(-16, 16))
			: new Vector2(512 + random.RandfRange(0, 32), playerTopLeft.Y + 2 + random.RandfRange(-16, 0));
		if (behavior == "zig_zag") _path = new CheckpointMovement(true, random);
		Color = random.RandiRange(0, 5);
	}

	public void Update(RandomNumberGenerator random)
	{
		if (_path != null) Position += _path.Step();
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
			Position.X -= 1.7f;
			Position.Y += random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20);
		}
		_ticks++;
	}
}
