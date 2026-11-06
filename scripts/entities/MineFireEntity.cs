using Godot;

// Persistent flame: alternate size each update and choose a random color.
public sealed class MineFireEntity
{
	private int _size;
	public Vector2 Position { get; }
	public int Frame { get; private set; }
	public MineFireEntity(Vector2 position) => Position = position;
	public void Update(RandomNumberGenerator random)
	{
		_size = (_size + 1) % 2;
		Frame = (random.RandiRange(0, 3) * 3 + _size) % 8;
	}
}
