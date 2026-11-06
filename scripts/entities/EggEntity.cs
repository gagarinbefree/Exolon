using Godot;
using System;
using System.Collections.Generic;

// The egg follows the original per-update movement and is confined until its
// incubator is destroyed. Standalone eggs start unconfined.
public sealed class EggEntity
{
	private readonly IncubatorEntity? _incubator;
	private Vector2 _velocity;
	private int _ticks;
	public Vector2 Position;
	public bool HasOwnView;
	public Rect2 Bounds => new(Position, new Vector2(16, 16));
	public bool Confined => _incubator != null && _incubator.Alive;
	public int Frame => _ticks / 3 % 2;

	public EggEntity(Vector2 position, IncubatorEntity? incubator, RandomNumberGenerator random)
	{
		Position = position;
		_incubator = incubator;
		_velocity = new Vector2(random.RandfRange(-3, 3), random.RandfRange(-1, 1));
	}

	public void Update(RandomNumberGenerator random, IReadOnlyList<Rect2> solidTiles)
	{
		Move(_velocity, solidTiles);
		Move(new Vector2(0, random.RandfRange(-0.5f, 0.5f)), solidTiles);
		int change = random.RandiRange(0, 6);
		if (change == 0) _velocity.X = -_velocity.X;
		else if (change == 1) _velocity.Y = -_velocity.Y;
		_ticks++;
	}

	private void Move(Vector2 step, IReadOnlyList<Rect2> solidTiles)
	{
		Vector2 old = Position;
		Position += step;
		if (Confined)
		{
			Rect2 box = _incubator!.EggBounds;
			if (Position.X < box.Position.X || Position.X + 16 > box.End.X) { Position.X = old.X; _velocity.X = -_velocity.X; }
			if (Position.Y < box.Position.Y || Position.Y + 16 > box.End.Y) { Position.Y = old.Y; _velocity.Y = -_velocity.Y; }
		}
		foreach (Rect2 tile in solidTiles)
		{
			if (!Bounds.Intersects(tile)) continue;
			Rect2 horizontal = new(new Vector2(Position.X, old.Y), new Vector2(16, 16));
			if (horizontal.Intersects(tile)) { Position.X = old.X; _velocity.X = -_velocity.X; }
			if (Bounds.Intersects(tile)) { Position.Y = old.Y; _velocity.Y = -_velocity.Y; }
		}
		if (Position.X < 0 || Position.X + 16 > 512) { Position.X = Mathf.Clamp(Position.X, 0, 496); _velocity.X = -_velocity.X; }
		if (Position.Y < 0) { Position.Y = 0; _velocity.Y = -_velocity.Y; }
	}
}
