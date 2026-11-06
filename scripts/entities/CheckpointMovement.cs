using Godot;

// melonJS checkpoint movement: each leg ends after the vector length has accumulated.
public sealed class CheckpointMovement
{
	private readonly Vector2[] _velocities;
	private readonly float[] _distances;
	private int _index;
	private float _distance;
	public int Phase => _index;

	public CheckpointMovement(bool zigZag, RandomNumberGenerator random)
	{
		if (zigZag)
		{
			_velocities = new[] { new Vector2(-2.5f, -3), new Vector2(-2.5f, 3), new Vector2(-2.5f, -3), new Vector2(0, 3), new Vector2(-2.5f, 0), new Vector2(0, -2.5f), new Vector2(2.5f, 0), new Vector2(0, 2.5f), new Vector2(-5, 0) };
			_distances = new float[] { 120, 120, 60, 40, 180, 60, 60, 60 };
		}
		else
		{
			_velocities = new[] { new Vector2(-3, 0), new Vector2(0, -1), new Vector2(0, 1), new Vector2(-4, 0) };
			_distances = new float[] { 176, 80, 80 - random.RandiRange(0, 32) };
		}
	}

	public Vector2 Step()
	{
		Vector2 step = _velocities[_index];
		_distance += step.Length();
		if (_index < _distances.Length && _distance >= _distances[_index]) { _distance = 0; _index++; }
		return step;
	}
}
