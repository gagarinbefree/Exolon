using Godot;
using System;

/// <summary>Movement and frames calibrated against the melonJS Vitorc controller.</summary>
public partial class Player : CharacterBody2D
{
	private const float WalkSpeed = 90f;
	private const float JumpSpeed = -165f;
	private const float Gravity = 360f;
	private const float MaxFallSpeed = 165f;
	private static readonly int[] WalkFrames = { 0, 1, 2, 3, 4, 0, 5, 6, 7, 8 };

	private Sprite2D _sprite = null!;
	private CollisionShape2D _collision = null!;
	private readonly RectangleShape2D _standingShape = new() { Size = new Vector2(46, 63) };
	private readonly RectangleShape2D _duckingShape = new() { Size = new Vector2(46, 52) };
	private int _direction = 1;
	private int _walkTick;
	private bool _jumping;
	private float _descendingDistance;
	public int Direction => _direction;
	public bool IsDucking { get; private set; }
	public bool IsDead { get; private set; }
	public bool IsInvincible { get; private set; }
	public bool IsArmored { get; private set; }
	public void ToggleOutfit() => IsArmored = !IsArmored;
	private float _invincibleTime;
	private Vector2 _spawn;
	public bool CanLeaveScreen { get; set; } = true;
	public Func<bool>? TryInteract { get; set; }
	private bool _interactHeld;

	public override void _Ready()
	{
		_spawn = Position;
		_sprite = new Sprite2D
		{
			Texture = GD.Load<Texture2D>("res://assets/images/vitorc2.png"),
			RegionEnabled = true,
			RegionRect = new Rect2(0, 0, 48, 64),
			TextureFilter = CanvasItem.TextureFilterEnum.Nearest
		};
		_collision = new CollisionShape2D { Shape = _standingShape, Position = new Vector2(0, 0.5f) };
		AddChild(_sprite);
		AddChild(_collision);
		FloorSnapLength = 1f;
		SafeMargin = 0.01f;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		if (IsInvincible && (_invincibleTime -= dt) <= 0) IsInvincible = false;
		if (IsDead)
		{
			Vector2 deadVelocity = Velocity;
			deadVelocity.Y = Mathf.Min(deadVelocity.Y + Gravity * dt, MaxFallSpeed);
			Velocity = deadVelocity;
			MoveAndSlide();
			return;
		}
		Vector2 velocity = Velocity;
		bool onGround = IsOnFloor();
		bool down = Input.IsActionPressed("ui_down");
		bool right = Input.IsActionPressed("ui_right");
		bool left = Input.IsActionPressed("ui_left");
		bool jump = Input.IsActionPressed("ui_up");
		if (!jump) _interactHeld = false;

		if (onGround)
		{
			_jumping = false;
			_descendingDistance = 0;
			velocity.X = 0;
			if (!down)
			{
				if (right) { _direction = 1; velocity.X = WalkSpeed; }
				else if (left) { _direction = -1; velocity.X = -WalkSpeed; }
				if (jump && !_interactHeld)
				{
					if (TryInteract?.Invoke() == true) _interactHeld = true;
					else { velocity.Y = JumpSpeed; _jumping = true; }
				}
			}
		}
		else if (_jumping && velocity.Y <= 0)
		{
			if ((right && _direction == 1) || (left && _direction == -1))
				velocity.X = _direction * WalkSpeed;
		}
		else if (!_jumping)
		{
			velocity.X = 0;
		}

		velocity.Y = Mathf.Min(velocity.Y + Gravity * dt, MaxFallSpeed);
		if (_jumping && velocity.Y > 0)
			_descendingDistance += Math.Abs(velocity.X) * dt;
		Velocity = velocity;
		MoveAndSlide();
		Position = new Vector2(Mathf.Clamp(Position.X, 24, CanLeaveScreen ? 540 : 488), Position.Y);

		bool ducking = IsOnFloor() && down;
		IsDucking = ducking;
		_collision.Shape = ducking ? _duckingShape : _standingShape;
		_collision.Position = ducking ? new Vector2(0, 6) : new Vector2(0, 0.5f);
		_sprite.FlipH = _direction < 0;
		int frame;
		if (!IsOnFloor()) frame = _jumping && _descendingDistance <= 50 ? 3 : 8;
		else if (ducking) frame = 9;
		else if (Math.Abs(Velocity.X) > 0.1f) { frame = WalkFrames[(_walkTick++ / 3) % WalkFrames.Length]; }
		else { frame = 0; _walkTick = 0; }
		if (IsArmored) frame += 11;
		_sprite.RegionRect = new Rect2(frame * 48, 0, 48, 64);
	}

	public bool Kill()
	{
		if (IsDead || IsInvincible) return false;
		IsDead = true;
		Velocity = new Vector2(0, JumpSpeed);
		_sprite.RegionRect = new Rect2((IsArmored ? 21 : 10) * 48, 0, 48, 64);
		return true;
	}

	public void Respawn()
	{
		Position = _spawn;
		Velocity = Vector2.Zero;
		IsDead = false;
		GrantInvulnerability(3f);
		_jumping = false;
	}

	public void GrantInvulnerability(float seconds)
	{
		IsInvincible = true;
		_invincibleTime = Mathf.Max(seconds, 0f);
	}

	public void EnterScreen(float x, float respawnY)
	{
		Position = new Vector2(x, Position.Y);
		_spawn = new Vector2(x, respawnY);
	}

	public void EnterLevel(float x, float y)
	{
		Position = new Vector2(x, y);
		_spawn = Position;
		Velocity = Vector2.Zero;
		IsDead = false; IsInvincible = false; IsArmored = false;
		_jumping = false; _direction = 1; _interactHeld = false;
		Show(); SetPhysicsProcess(true);
	}
}
