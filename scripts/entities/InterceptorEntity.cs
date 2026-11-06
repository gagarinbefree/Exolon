using Godot;

// Default SwingAndAccelerationMovementBehavior from melonJS.
public sealed class InterceptorEntity
{
	private static readonly int[] Frames = { 0, 1, 2, 1 };
	private readonly int _color;
	private int _ticks;
	private int _straightTicks;
	private readonly bool _acceleration;
	private readonly CheckpointMovement? _path;
	public Vector2 Position;
	public bool Fast => _acceleration ? Position.X < 304 : _straightTicks > 40;
	public int Frame => _color * 3 + Frames[_ticks / 3 % Frames.Length];
	public Rect2 Bounds => new(Position, new Vector2(32, 32));

	public InterceptorEntity(Vector2 playerTopLeft, RandomNumberGenerator random, string behavior = "")
	{
		_acceleration = behavior == "acceleration";
		Position = behavior == "zig_zag" ? new Vector2(512, playerTopLeft.Y + 2) : _acceleration
			? new Vector2(512, playerTopLeft.Y + 2 + random.RandfRange(-32, 32))
			: new Vector2(512 + random.RandfRange(0, 32), playerTopLeft.Y + 2 + random.RandfRange(-4, 0));
		if (behavior == "zig_zag") _path = new CheckpointMovement(true, random);
		_color = random.RandiRange(0, 5);
	}
	public void Update(RandomNumberGenerator random)
	{
		if (_path != null) { Position += _path.Step(); _ticks++; return; }
		if (!_acceleration)
		{
			if (Position.X >= 336) Position.Y += random.RandfRange(1, 3) * Mathf.Sin(Position.X / 20);
			else _straightTicks++;
		}
		Position.X -= Fast ? 4 : 1.5f;
		_ticks++;
	}
}
