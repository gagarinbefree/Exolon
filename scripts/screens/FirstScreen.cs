using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Screen assets and collision data imported from the melonJS maps.
[Tool]
public partial class FirstScreen : Node2D
{
	[Export] public string InitialLevel { get; set; } = "L01S01";
	[Export] public int InitialZoneCount { get; set; }
	private sealed record TileSetInfo(uint FirstGid, int Width, int Height, Texture2D Texture);
	private readonly List<TileSetInfo> _tileSets = new();
	private readonly Dictionary<string, Texture2D> _textures = new();
	private readonly List<(int X, int Y, uint Gid)> _tiles = new();
	private readonly List<(int X, int Y, uint Gid, int Z)> _foregroundTiles = new();
	private readonly List<MapObjectView> _objectViews = new();
	private MapObjectView _dynamicView = null!;
	private const int PlayerZ = 2048;
	private Node2D _foreground = null!;
	private readonly List<(string Name, int X, int Y, string Placement)> _objects = new();
	private readonly Dictionary<(string Name, int X, int Y), uint> _objectGids = new();
	private readonly List<(int X, int Y, uint Gid)> _stars = new();
	private readonly List<TeleportEntity> _teleports = new();
	private readonly List<(int X, int Bottom, PistonEntity Piston)> _pistons = new();
	private readonly List<CapsuleEntity> _capsules = new();
	private BubbleCreatorEntity? _bubbleCreator;
	private InterceptorCreatorEntity? _interceptorCreator;
	private JellyfishCreatorEntity? _jellyfishCreator;
	private LouseCreatorEntity? _louseCreator;
	private FlyingCreatorEntity? _flyingCreator;
	private readonly List<BubbleEntity> _bubbles = new();
	private readonly List<InterceptorEntity> _interceptors = new();
	private readonly List<JellyfishEntity> _jellyfish = new();
	private readonly List<LouseEntity> _lice = new();
	private readonly List<FlyingEnemyEntity> _flyingEnemies = new();
	private readonly List<HarbringerEntity> _harbringers = new();
	private float _harbringerTimer;
	private readonly List<EggEntity> _eggs = new();
	private readonly List<DoubleLauncherEntity> _launchers = new();
	private readonly List<AwardPointsEntity> _awards = new();
	private readonly List<MineEntity> _mines = new();
	private readonly List<MineFireEntity> _mineFires = new();
	private readonly List<Rect2> _exits = new();
	private readonly Dictionary<Vector2I, int> _dischargeFrames = new();
	private LevelCompleteWindowEntity? _completeWindow;
	private BonusScreenEntity? _bonusScreen;
	private bool _newLevelAfterBonus;
	private MissileEntity? _missile;
	private int _missileSpawnCount;
	private int _launcherShotsFired;
	private readonly List<Rect2> _groundRects = new();
	private sealed class Shot
	{
		public string Kind = "";
		public Vector2 Position;
		public Vector2 Velocity;
		public int Direction;
		public float Age;
		public float Distance;
		public bool Destroyed;
	}
	private readonly List<ObstacleEntity> _obstacles = new();
	private readonly List<Shot> _shots = new();
	private sealed class Effect
	{
		public string Kind = "";
		public Vector2 Position;
		public Vector2 Velocity;
		public float Age;
		public float Duration;
		public bool Flip;
		public int Frame;
		public Vector2 Acceleration;
		public Vector2 MaxVelocity;
	}
	private readonly List<Effect> _effects = new();
	private readonly List<StaticBody2D> _ground = new();
	private readonly RandomNumberGenerator _random = new();
	private Player _player = null!;
	private int _ammo = 99, _grenades = 10, _lives = 9, _points;
	private float _fireHeld, _deathTime;
	private bool _fireWasPressed;
	private bool _gameOver;
	private float _gameOverTime;
	private int _zone;
	private string _nextLevel = "";
	private bool _transitionPending;
	private bool _grenadePackCollected;
	private bool _ammoPackCollected;
	private readonly Dictionary<Vector2I, int> _shipFireFrames = new();
	private int _ambientTicks;
	private int _lightFrame;
	private int _shipFireSize;
	public int TestLightFrame() => _lightFrame;
	public int TestBeamFrame() => _ambientTicks / 3 % 8;
	public int TestShipFireFrame(int x, int y) => _shipFireFrames.GetValueOrDefault(new Vector2I(x, y), -1);
	public int TestPoints() => _points;
	public int TestLives() => _lives;
	public int TestAmmo() => _ammo;
	public int TestGrenades() => _grenades;
	public int TestZone() => _zone;
	public bool TestObstacleAlive(string name) => _obstacles.Exists(o => o.Name == name && o.Alive);
	public int TestObstacleCount(string name) => _obstacles.FindAll(o => o.Name == name && o.Alive).Count;
	public bool TestGrenadePackCollected() => _grenadePackCollected;
	public string TestNextLevel() => _nextLevel;
	public bool TestAmmoPackCollected() => _ammoPackCollected;
	public float TestPistonRise() => _pistons.Count > 0 ? _pistons[0].Piston.Rise : 0;
	public int TestPistonCount() => _pistons.Count;
	public float TestPistonRiseAt(int index) => index >= 0 && index < _pistons.Count ? _pistons[index].Piston.Rise : -1;
	public int TestBlasterBulletCount() => _shots.Count(shot => shot.Kind == "blaster_bullet" && !shot.Destroyed);
	public Vector2 TestBlasterBulletPosition(int index) => _shots.Where(shot => shot.Kind == "blaster_bullet" && !shot.Destroyed).Skip(index).FirstOrDefault()?.Position ?? new Vector2(-100, -100);
	public int TestBubbleCount() => _bubbles.Count;
	public int TestInterceptorCount() => _interceptors.Count;
	public int TestJellyfishCount() => _jellyfish.Count;
	public int TestLouseCount() => _lice.Count;
	public int TestFlyingEnemyCount() => _flyingEnemies.Count;
	public int TestHarbringerCount() => _harbringers.Count;
	public Vector2 TestFirstHarbringerPosition() => _harbringers.Count > 0 ? _harbringers[0].Position : new Vector2(-100, -100);
	public Vector2 TestFirstFlyingPosition() => _flyingEnemies.Count > 0 ? _flyingEnemies[0].Position : new Vector2(-100, -100);
	public int TestFirstFlyingPhase() => _flyingEnemies.Count > 0 ? _flyingEnemies[0].Phase : -1;
	public Vector2 TestFirstLousePosition() => _lice.Count > 0 ? _lice[0].Position : new Vector2(-100, -100);
	public bool TestFirstLouseFast() => _lice.Count > 0 && _lice[0].Fast;
	public bool TestFirstLouseCircular() => _lice.Count > 0 && _lice[0].Circular;
	public Vector2 TestFirstJellyfishPosition() => _jellyfish.Count > 0 ? _jellyfish[0].Position : new Vector2(-100, -100);
	public bool TestFirstJellyfishCircular() => _jellyfish.Count > 0 && _jellyfish[0].Circular;
	public bool TestFirstJellyfishFast() => _jellyfish.Count > 0 && _jellyfish[0].Fast;
	public bool TestHasInterceptorCreator() => _interceptorCreator != null;
	public bool TestHasBubbleCreator() => _bubbleCreator != null;
	public bool TestHasLouseCreator() => _louseCreator != null;
	public int TestCapsuleCount() => _capsules.Count;
	public int TestTeleportCount() => _teleports.Count;
	public int TestObjectZ(string name, int x, int y)
	{
		if (name == "teleport") return _teleports.Find(t => t.Position == new Vector2(x, y - 96))?.ZIndex ?? -1;
		return _objectViews.Find(v => v.ObjectName == name && v.MapPosition == new Vector2I(x, y))?.ZIndex ?? -1;
	}
	public int TestLauncherCount() => _launchers.Count;
	public int TestLauncherDuration(int index) => index >= 0 && index < _launchers.Count ? _launchers[index].Duration : -1;
	public int TestLauncherDurationMax(int index) => index >= 0 && index < _launchers.Count ? _launchers[index].DurationMax : -1;
	public bool TestLevelCompleteVisible() => _completeWindow != null;
	public bool TestBonusScreenVisible() => _bonusScreen != null;
	public int TestMapObjectCount(string name) => _objects.Count(obj => obj.Name == name);
	public Vector2 TestFirstInterceptorPosition() => _interceptors.Count > 0 ? _interceptors[0].Position : new Vector2(-100, -100);
	public bool TestFirstInterceptorFast() => _interceptors.Count > 0 && _interceptors[0].Fast;
	public int TestEggCount() => _eggs.Count;
	public int TestMineCount() => _mines.Count(mine => !mine.Triggered);
	public int TestMineFireCount() => _mineFires.Count;
	public int TestMineFireFrame(int index) => index >= 0 && index < _mineFires.Count ? _mineFires[index].Frame : -1;
	public Vector2 TestMissilePosition() => _missile?.Position ?? new Vector2(-100, -100);
	public bool TestMissileFast() => _missile?.Fast ?? false;
	public int TestMissileSpawnCount() => _missileSpawnCount;
	public int TestTurretBulletsAtY(float y) => _shots.Count(shot => shot.Kind == "turret_bullet" && Math.Abs(shot.Position.Y - y) < 0.1f);
	public int TestConfinedEggCount() => _eggs.Count(egg => egg.Confined);
	public bool TestLauncherCaptured() => _launchers.Exists(launcher => launcher.Captured);
	public int TestLauncherBulletCount() => _shots.Count(shot => shot.Kind == "double_launcher_bullet" && !shot.Destroyed);
	public int TestLauncherShotsFired() => _launcherShotsFired;
	public Vector2 TestLauncherBulletPosition() => _shots.Find(shot => shot.Kind == "double_launcher_bullet" && !shot.Destroyed)?.Position ?? new Vector2(-100, -100);
	public int TestAwardRemaining() => _awards.Sum(award => award.Remaining);
	public Vector2 TestFirstEggPosition() => _eggs.Count > 0 ? _eggs[0].Position : new Vector2(-100, -100);
	public Vector2 TestFirstBubblePosition() => _bubbles.Count > 0 ? _bubbles[0].Position : new Vector2(-100, -100);
	public bool TestFirstBubbleCircular() => _bubbles.Count > 0 && _bubbles[0].Circular;
	public int TestTeleportFlashCount() => GetChildren().Count(node => node is TeleportFlashEntity);
	private readonly Dictionary<string, List<AudioStreamPlayer>> _audioChannels = new();
	public void TestPlaySound(string name) => PlaySound(name);
	public int TestActiveAudioChannels(string name) => _audioChannels.TryGetValue(name, out var channels) ? channels.Count(channel => channel.Playing) : 0;
	public Vector2 TestGrenadePosition()
	{
		Shot? grenade = _shots.Find(shot => shot.Kind == "grenade");
		return grenade?.Position ?? new Vector2(-100, -100);
	}
	public Vector2 TestTurretBulletPosition()
	{
		Shot? bullet = _shots.Find(shot => shot.Kind == "turret_bullet");
		return bullet?.Position ?? new Vector2(-100, -100);
	}
	public Vector2 TestTracePosition() => _effects.Find(effect => effect.Kind == "grenade_trace")?.Position ?? new Vector2(-100, -100);
	public int TestTraceFrame()
	{
		Effect? effect = _effects.Find(effect => effect.Kind == "grenade_trace");
		return effect == null ? -1 : Mathf.Min((int)(effect.Age / effect.Duration * 28), 27);
	}

	public override void _Ready()
	{
		// The editor shows the same map, but never runs the game simulation.
		if (Engine.IsEditorHint()) SetPhysicsProcess(false);
		_foreground = new Node2D { Name = "Foreground", ZIndex = 0 };
		AddChild(_foreground);
		_dynamicView = new MapObjectView { Name = "DynamicObjects", DrawObject = DrawDynamicObjects };
		AddChild(_dynamicView);
		AddChild(new MapObjectView { Name = "EnemyExplosions", ZIndex = PlayerZ + 1, DrawObject = target => DrawEffects(target, "circular_explosion") });
		AddChild(new MapObjectView { Name = "ExplosionParticles", ZIndex = 3800, DrawObject = target => DrawEffects(target, "explosion_particle") });
		AddChild(new MapObjectView { Name = "HUD", ZIndex = 3900, DrawObject = DrawHud });
		_zone = InitialZoneCount;
		LoadMap(InitialLevel);
		if (Engine.IsEditorHint()) { QueueRedraw(); return; }
		_player.CanLeaveScreen = LevelMap.Exists(_nextLevel) && LevelMap.CanEnter(_nextLevel);
		QueueRedraw();
	}

	public override void _ExitTree()
	{
		foreach (List<AudioStreamPlayer> channels in _audioChannels.Values)
			foreach (AudioStreamPlayer channel in channels)
				if (IsInstanceValid(channel)) { channel.Stop(); channel.Stream = null; }
		_audioChannels.Clear();
	}

	private Texture2D Texture(string name)
	{
		if (!_textures.TryGetValue(name, out Texture2D? texture))
		{
			texture = GD.Load<Texture2D>($"res://assets/images/{name}.png")
				?? throw new InvalidOperationException($"Missing texture: {name}");
			_textures.Add(name, texture);
		}
		return texture;
	}

	private void LoadMap(string level)
	{
		LevelMap map = LevelMap.Load(level);
		_dynamicView.ZIndex = PlayerZ + map.DynamicDrawOrder;
		_nextLevel = map.NextLevel;
		foreach (var set in map.Tilesets)
			_tileSets.Add(new(set.FirstGid, set.Width, set.Height, Texture(set.Name)));
		foreach (var tile in map.Tiles)
		{
			if (tile.Layer == "Collision") { if (!Engine.IsEditorHint()) AddGroundCollision(tile.X, tile.Y); }
			else if (tile.Layer != "Stars")
				if (tile.Foreground) _foregroundTiles.Add((tile.X, tile.Y, tile.Gid, PlayerZ + tile.DrawOrder));
				else _tiles.Add((tile.X, tile.Y, tile.Gid));
		}
		foreach (var (x, y, gid, z) in _foregroundTiles) AddForegroundTile(x, y, gid, z);

		foreach (var obj in map.Objects)
		{
			EggEntity? mapEgg = null;
			string name = obj.Name;
			_objectGids[(name, obj.X, obj.Y)] = obj.Gid;
			string placement = obj.Placement;
			if (name == "vitorc")
			{
				if (Engine.IsEditorHint())
				{
					_objects.Add((name, obj.X, obj.Y, placement));
					AddObjectView(obj);
					continue;
				}
				float spawnX = obj.X + 24;
				float spawnY = obj.Y - 32;
				if (_player == null)
				{
					_player = new Player { Name = "Player", ZIndex = PlayerZ, Position = new Vector2(spawnX, spawnY), TryInteract = TryInteract };
					AddChild(_player);
				}
				else if (_newLevelAfterBonus) _player.EnterLevel(spawnX, spawnY);
				else _player.EnterScreen(spawnX, spawnY);
				continue;
			}
			if (!Engine.IsEditorHint() && name is ("turret" or "cocoon" or "radar" or "rocket" or "incubator" or "missile_guidance" or "waggon" or "combined_launcher_top" or "fungus" or "beam"))
				AddObstacle(name, obj.X, obj.Y);
			if (!Engine.IsEditorHint() && name == "egg")
			{
				mapEgg = new EggEntity(new Vector2(obj.X, obj.Y - 16), null, _random) { HasOwnView = true };
				_eggs.Add(mapEgg);
			}
			if (name == "teleport")
			{
				var teleport = new TeleportEntity { Position = new Vector2(obj.X, obj.Y - 96), ZIndex = PlayerZ + obj.DrawOrder };
				AddChild(teleport);
				_teleports.Add(teleport);
			}
			if (name == "piston" && !Engine.IsEditorHint()) _pistons.Add((obj.X, obj.Y, new PistonEntity(_random)));
			if (name == "capsule" && !Engine.IsEditorHint()) _capsules.Add(new CapsuleEntity(obj.X, obj.Y, obj.Width, obj.Height));
			if (name == "exit" && !Engine.IsEditorHint()) _exits.Add(new Rect2(obj.X, obj.Y, obj.Width, obj.Height));
			if (name == "discharge") _dischargeFrames[new Vector2I(obj.X, obj.Y)] = 0;
			if (name == "bubble_creator" && !Engine.IsEditorHint()) _bubbleCreator = new BubbleCreatorEntity { Behavior = obj.Behavior, Delay = obj.Delay > 0 ? obj.Delay : 1f };
			if (name == "interceptor_creator" && !Engine.IsEditorHint()) _interceptorCreator = new InterceptorCreatorEntity { Behavior = obj.Behavior, Delay = obj.Delay > 0 ? obj.Delay : 1.5f };
			if (name == "jellyfish_creator" && !Engine.IsEditorHint()) _jellyfishCreator = new JellyfishCreatorEntity { Behavior = obj.Behavior, Delay = obj.Delay > 0 ? obj.Delay : 1f };
			if (name == "louse_creator" && !Engine.IsEditorHint()) _louseCreator = new LouseCreatorEntity { Behavior = obj.Behavior, Delay = obj.Delay > 0 ? obj.Delay : 0.8f };
			if (name is "fir_creator" or "flasher_creator" && !Engine.IsEditorHint()) _flyingCreator = new FlyingCreatorEntity(name == "fir_creator" ? "fir" : "flasher", obj.Behavior, obj.Delay);
			if (name == "double_launcher" && !Engine.IsEditorHint()) _launchers.Add(new DoubleLauncherEntity(obj.X, obj.Y, _random, durationMin: obj.FireDurationMin, durationMax: obj.FireDurationMax));
			if (name == "combined_launcher_bottom" && !Engine.IsEditorHint()) _launchers.Add(new CombinedLauncherBottomEntity(obj.X, obj.Y, _random));
			if (name == "mine" && !Engine.IsEditorHint()) _mines.Add(new MineEntity(obj.X, obj.Y));
			if (name == "ship_fire") _shipFireFrames[new Vector2I(obj.X, obj.Y)] = 0;
			_objects.Add((name, obj.X, obj.Y, placement));
			if (name != "teleport")
			{
				MapObjectView view = AddObjectView(obj);
				if (mapEgg != null)
				{
					EggEntity egg = mapEgg;
					view.DrawObject = target => { if (_eggs.Contains(egg)) DrawFrameOn(target, "egg", (int)egg.Position.X, (int)egg.Position.Y, 16, 16, egg.Frame); };
				}
			}
		}
		var random = new RandomNumberGenerator { Seed = 1987 };
		for (int i = 0; i < 50; i++)
			_stars.Add((random.RandiRange(0, 31) * 16, random.RandiRange(0, 17) * 16, (uint)random.RandiRange(10, 14)));
	}

	private void AddForegroundTile(int x, int y, uint gid, int z)
	{
		TileSetInfo? chosen = null;
		foreach (TileSetInfo set in _tileSets)
			if (set.FirstGid <= gid) chosen = set;
		if (chosen == null) return;
		int index = (int)(gid - chosen.FirstGid);
		int columns = chosen.Texture.GetWidth() / chosen.Width;
		_foreground.AddChild(new Sprite2D
		{
			Texture = chosen.Texture,
			ZIndex = z,
			Centered = false,
			Position = new Vector2(x, y + 16 - chosen.Height),
			RegionEnabled = true,
			RegionRect = new Rect2(index % columns * chosen.Width, index / columns * chosen.Height, chosen.Width, chosen.Height)
		});
	}

	private void AddObstacle(string name, int x, int bottom)
	{
		ObstacleEntity obstacle = name switch {
			"turret" => new TurretEntity { Bounds = new Rect2(x + 40, bottom - 80, 48, 80) },
			"rocket" => new RocketEntity(x, bottom),
			"radar" => new RadarEntity(x, bottom),
			"incubator" => new IncubatorEntity(x, bottom),
			"missile_guidance" => new MissileGuidanceEntity(x, bottom),
			"waggon" => new WaggonEntity(x, bottom),
			"combined_launcher_top" => new CombinedLauncherTopEntity(x, bottom),
			"fungus" => new FungusEntity(x, bottom),
			"beam" => new BeamEntity(x, bottom),
			_ => new ObstacleEntity { Bounds = new Rect2(x, bottom - 128, 80, 128) }
		};
		Rect2 bounds = obstacle.Bounds;
		var body = new StaticBody2D { Position = bounds.GetCenter() };
		body.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = bounds.Size } });
		Rect2 tube = name == "turret" ? new Rect2(x, bottom - 64, 32, 16) : name == "combined_launcher_top" ? new Rect2(x - 16, bottom - 32, 32, 16) : default;
		if (tube.HasArea()) body.AddChild(new CollisionShape2D { Position = tube.GetCenter() - bounds.GetCenter(), Shape = new RectangleShape2D { Size = tube.Size } });
		AddChild(body);
		obstacle.Name = name; obstacle.Bounds = bounds; obstacle.TubeBounds = tube; obstacle.Body = body; obstacle.MapX = x; obstacle.MapBottom = bottom;
		if (obstacle is TurretEntity turret) turret.ResetTimer(_random);
		if (obstacle is IncubatorEntity incubator)
			for (int i = 0; i < 8; i++) _eggs.Add(new EggEntity(incubator.Bounds.Position + new Vector2(20, 20), incubator, _random));
		_obstacles.Add(obstacle);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		if (_gameOver)
		{
			_gameOverTime += dt;
			if (_gameOverTime >= 4f) GetTree().ChangeSceneToFile("res://scenes/screens/title_screen.tscn");
			return;
		}
		bool completing = _completeWindow != null;
		if (completing)
		{
			if (_bonusScreen != null) return;
			if (_awards.Count == 0 && Input.IsActionJustPressed("fire")) { StartBonusScreen(); return; }
		}
		if (!completing && LevelMap.Exists(_nextLevel) && LevelMap.CanEnter(_nextLevel) && !_player.IsDead && _player.Position.X > 534 && !_transitionPending)
		{
			_transitionPending = true;
			Callable.From(AdvanceScreen).CallDeferred();
			return;
		}
		if (!completing && _player.IsDead)
		{
			if (_player.IsOnFloor()) _deathTime += dt;
			if (_deathTime > 70f / 60f)
			{
				_deathTime = 0;
				_lives--;
				if (_lives <= 0) { ShowGameOver(); return; }
				_ammo = 99; _grenades = 10;
				_player.Respawn();
			}
		}
		else if (!completing) HandleFire(dt);

		foreach (ObstacleEntity obstacle in _obstacles)
		{
			if (!completing && obstacle is BeamEntity && obstacle.Alive && obstacle.Bounds.Grow(2).Intersects(new Rect2(_player.Position - new Vector2(23, 31), new Vector2(46, 63))))
				if (_player.Kill()) _deathTime = 0;
			if (obstacle is TurretEntity turret && turret.Alive && turret.Update(_random))
			{
				// melonJS: tube top + 10, then ObjectEntity subtracts bullet height (4).
				_shots.Add(new Shot { Kind = "turret_bullet", Position = new Vector2(turret.TubeBounds.Position.X - 4, turret.TubeBounds.Position.Y + 6), Velocity = new Vector2(-300, 0) });
				PlaySound("shot1");
			}
		}
		UpdateShots(dt);
		UpdateLaunchers(dt);
		UpdateBubbles(dt);
		UpdateInterceptors(dt);
		UpdateJellyfish(dt);
		UpdateLice(dt);
		UpdateFlyingEnemies(dt);
		UpdateHarbringers(dt);
		UpdateEggs();
		UpdateMissile();
		UpdateMines();
		UpdateEffects(dt);
		if (!completing) { CollectGrenadePack(); CollectAmmoPack(); }
		UpdatePiston(dt);
		UpdateAmbientAnimations();
		if (!completing) UpdateExit();
		QueueRedraw();
	}

	private void ShowGameOver()
	{
		_gameOver = true;
		_player.SetPhysicsProcess(false);
		AddChild(new GameOverWindow { Name = "GameOverWindow", ZIndex = 4000 });
		PlaySound("gameover");
	}

	private void UpdateAmbientAnimations()
	{
		// melonJS runs at 60 fps: lights advance every three updates and
		// each ship flame changes size and independently chooses a color every two.
		_ambientTicks++;
		if (_ambientTicks % 3 == 0) _lightFrame = (_lightFrame + 1) % 8;
		foreach (var (name, x, y, _) in _objects)
			if (name == "discharge") _dischargeFrames[new Vector2I(x, y)] = _random.RandiRange(0, 39);
		if (_ambientTicks % 2 != 0) return;
		_shipFireSize = (_shipFireSize + 1) % 3;
		foreach (var (name, x, y, _) in _objects)
		{
			if (name == "ship_fire") _shipFireFrames[new Vector2I(x, y)] = _random.RandiRange(0, 3) * 3 + _shipFireSize;
		}
	}

	private void AdvanceScreen()
	{
		foreach (AwardPointsEntity award in _awards) _points += award.Remaining;
		_awards.Clear(); _launchers.Clear(); _mines.Clear(); _mineFires.Clear(); _exits.Clear(); _dischargeFrames.Clear(); _launcherShotsFired = 0; _missile = null; _missileSpawnCount = 0;
		foreach (StaticBody2D body in _ground) { RemoveChild(body); body.QueueFree(); }
		foreach (ObstacleEntity obstacle in _obstacles)
			if (IsInstanceValid(obstacle.Body) && !obstacle.Body.IsQueuedForDeletion()) { RemoveChild(obstacle.Body); obstacle.Body.QueueFree(); }
		_ground.Clear(); _groundRects.Clear(); _obstacles.Clear(); _shots.Clear(); _effects.Clear(); _eggs.Clear();
		foreach (TeleportEntity teleport in _teleports) { RemoveChild(teleport); teleport.QueueFree(); }
		foreach (MapObjectView view in _objectViews) { RemoveChild(view); view.QueueFree(); }
		_objectViews.Clear();
		_teleports.Clear(); _pistons.Clear(); _capsules.Clear(); _bubbleCreator = null; _interceptorCreator = null; _jellyfishCreator = null; _louseCreator = null; _flyingCreator = null; _bubbles.Clear(); _interceptors.Clear(); _jellyfish.Clear(); _lice.Clear(); _flyingEnemies.Clear(); _harbringers.Clear(); _harbringerTimer = 0;
		_tiles.Clear(); _foregroundTiles.Clear(); _tileSets.Clear(); _objects.Clear(); _objectGids.Clear(); _stars.Clear();
		_shipFireFrames.Clear(); _ambientTicks = 0; _lightFrame = 0; _shipFireSize = 0;
		foreach (Node child in _foreground.GetChildren()) { _foreground.RemoveChild(child); child.QueueFree(); }
		_zone++;
		_grenadePackCollected = false;
		_ammoPackCollected = false;
		LoadMap(_nextLevel);
		_newLevelAfterBonus = false;
		_player.CanLeaveScreen = LevelMap.Exists(_nextLevel) && LevelMap.CanEnter(_nextLevel);
		_transitionPending = false;
		QueueRedraw();
	}

	private bool TryInteract()
	{
		var teleports = _objects.FindAll(o => o.Name == "teleport");
		Rect2 player = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = 0; teleports.Count == 2 && i < 2; i++)
		{
			var source = teleports[i];
			Rect2 trigger = new(source.X + 16, source.Y - 64, 32, 48);
			if (!player.Intersects(trigger)) continue;
			var dest = teleports[1 - i];
			foreach (var teleport in teleports)
				AddChild(new TeleportFlashEntity { Position = new Vector2(teleport.X + 16, teleport.Y - 64), ZIndex = PlayerZ + 1 });
			_player.Position = new Vector2(dest.X + 24, dest.Y - 32);
			PlaySound("teleport");
			return true;
		}
		foreach (CapsuleEntity capsule in _capsules)
		{
			if (!player.Intersects(capsule.Bounds)) continue;
			_player.ToggleOutfit();
			PlaySound("ding");
			return true;
		}
		return false;
	}

	private void CollectAmmoPack()
	{
		if (_ammoPackCollected || _player.IsDead) return;
		Rect2 player = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		foreach (var (name, x, y, _) in _objects)
		{
			if (name != "ammo_pack" || !player.Intersects(new Rect2(x + 2, y - 30, 28, 30))) continue;
			_ammoPackCollected = true;
			_ammo = 99;
			PlaySound("pick2");
			break;
		}
	}

	private void UpdatePiston(float dt)
	{
		Rect2 player = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		foreach (var (x, bottom, piston) in _pistons)
		{
			piston.Update(dt, _random);
			float rise = piston.Rise;
			if (_completeWindow == null && rise > 0 && !_player.IsArmored && player.Intersects(new Rect2(x, bottom - rise, 48, rise)))
				if (_player.Kill()) _deathTime = 0;
		}
	}

	private void CollectGrenadePack()
	{
		if (_grenadePackCollected || _player.IsDead) return;
		Rect2 player = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		foreach (var (name, x, y, _) in _objects)
		{
			if (name != "grenade_pack" || !player.Intersects(new Rect2(x + 2, y - 30, 28, 30))) continue;
			_grenadePackCollected = true;
			_grenades = 10;
			PlaySound("pick1");
			break;
		}
	}

	private void HandleFire(float dt)
	{
		bool pressed = Input.IsActionPressed("fire");
		if (!pressed) { _fireHeld = 0; _fireWasPressed = false; return; }
		Vector2 topLeft = _player.Position - new Vector2(24, 32);
		float duckOffset = _player.IsDucking ? 10 : 0;
		int dir = _player.Direction;
		if (!_fireWasPressed && _ammo > 0)
		{
			float x = dir > 0 ? topLeft.X + 50 : topLeft.X - 18;
			_shots.Add(new Shot { Kind = "blaster_bullet", Position = new Vector2(x, topLeft.Y + 28 + duckOffset), Velocity = new Vector2(dir * 360, 0), Direction = dir });
			if (_player.IsArmored)
				_shots.Add(new Shot { Kind = "blaster_bullet", Position = new Vector2(x, topLeft.Y + 40 + duckOffset), Velocity = new Vector2(dir * 360, 0), Direction = dir });
			_ammo--; PlaySound("shot2");
		}
		_fireWasPressed = true;
		_fireHeld += dt;
		if (_fireHeld >= 35f / 60f && _grenades > 0 && !_shots.Exists(s => s.Kind == "grenade" || s.Kind == "blaster_bullet"))
		{
			// melonJS ObjectEntity converts bottom-left coordinates to sprite top-left.
			// GrenadeEntity is 16 px high and GrenadeTraceEntity is 48 px high.
			_shots.Add(new Shot { Kind = "grenade", Position = new Vector2(topLeft.X + (dir > 0 ? 20 : 12), topLeft.Y + 12 + duckOffset - 16), Velocity = new Vector2(dir * 90, -198), Direction = dir });
			_effects.Add(new Effect { Kind = "grenade_trace", Position = new Vector2(topLeft.X + (dir > 0 ? 4 : -20), topLeft.Y + 10 + duckOffset - 48), Duration = 56f / 60f, Flip = dir < 0 });
			_grenades--; PlaySound("grenade");
		}
	}

	private void UpdateShots(float dt)
	{
		for (int i = _shots.Count - 1; i >= 0; i--)
		{
			Shot shot = _shots[i]; shot.Age += dt;
			if (shot.Destroyed) { _shots.RemoveAt(i); continue; }
			if (shot.Kind == "grenade")
			{
				if (shot.Velocity.Y < 0) shot.Velocity.Y = Mathf.Min(0, shot.Velocity.Y + 720 * dt);
				else if (shot.Age > 0.78f) { shot.Velocity.X = shot.Direction * 330; shot.Velocity.Y += 360 * dt; }
			}
			Vector2 step = shot.Velocity * dt;
			shot.Position += step; shot.Distance += Math.Abs(step.X);
			Vector2 size = shot.Kind is "grenade" or "double_launcher_bullet" ? new Vector2(16, 16) : shot.Kind == "turret_bullet" ? new Vector2(4, 4) : new Vector2(16, 2);
			var rect = new Rect2(shot.Position, size);
			bool remove = shot.Position.X < -20 || shot.Position.X > 530 || shot.Position.Y > 288 || (shot.Kind == "blaster_bullet" && shot.Distance > 210);
			if (shot.Kind == "blaster_bullet")
			{
				foreach (Shot other in _shots)
				{
					if (other.Kind != "double_launcher_bullet" || other.Destroyed || !rect.Intersects(new Rect2(other.Position, new Vector2(16, 16)))) continue;
					other.Destroyed = true; remove = true; _points += 50;
					_effects.Add(new Effect { Kind = "blaster_explosion", Position = other.Position, Duration = 0.35f });
					PlaySound("burst"); break;
				}
				for (int j = _bubbles.Count - 1; j >= 0; j--)
				{
					if (remove) break;
					if (!_bubbles[j].Bounds.Intersects(rect)) continue;
					BurstBubble(j); remove = true; break;
				}
				if (!remove) for (int j = _interceptors.Count - 1; j >= 0; j--)
				{
					if (!_interceptors[j].Bounds.Intersects(rect)) continue;
					BurstInterceptor(j); remove = true; break;
				}
				if (!remove) for (int j = _jellyfish.Count - 1; j >= 0; j--)
				{
					if (!_jellyfish[j].Bounds.Intersects(rect)) continue;
					BurstJellyfish(j); remove = true; break;
				}
				if (!remove) for (int j = _lice.Count - 1; j >= 0; j--)
				{
					if (!_lice[j].Bounds.Intersects(rect)) continue;
					BurstLouse(j); remove = true; break;
				}
				if (!remove) for (int j = _eggs.Count - 1; j >= 0; j--)
				{
					if (!_eggs[j].Bounds.Intersects(rect)) continue;
					BurstEgg(j); remove = true; break;
				}
				if (!remove) for (int j = _flyingEnemies.Count - 1; j >= 0; j--)
				{
					if (!_flyingEnemies[j].Bounds.Intersects(rect)) continue;
					BurstFlyingEnemy(j); remove = true; break;
				}
			}
			if (_completeWindow == null && shot.Kind is ("turret_bullet" or "double_launcher_bullet") && rect.Intersects(new Rect2(_player.Position - new Vector2(23, 31), new Vector2(46, 63))))
			{
				if (_player.Kill()) _deathTime = 0;
				if (shot.Kind == "double_launcher_bullet") { _effects.Add(new Effect { Kind = "blaster_explosion", Position = shot.Position, Duration = 0.35f }); PlaySound("burst"); }
				remove = true;
			}
			if (!remove && shot.Kind == "double_launcher_bullet" && (_groundRects.Exists(tile => tile.Intersects(rect)) || _obstacles.Exists(obstacle => obstacle.Alive && obstacle.Intersects(rect))))
			{
				_effects.Add(new Effect { Kind = "blaster_explosion", Position = shot.Position, Duration = 0.35f }); PlaySound("burst"); remove = true;
			}
			if (!remove && shot.Kind is "grenade" or "blaster_bullet") foreach (ObstacleEntity obstacle in _obstacles)
			{
				if (!obstacle.Alive || !obstacle.Intersects(rect)) continue;
				remove = true;
				if (obstacle is BeamEntity beam)
				{
					if (shot.Kind == "blaster_bullet" && --beam.HitPoints <= 0)
					{
						beam.Alive = false; beam.Body.QueueFree(); SpawnExplosion(beam.Bounds.GetCenter());
						_awards.Add(new AwardPointsEntity(1000)); PlaySound("points");
					}
					else { _effects.Add(new Effect { Kind = "blaster_explosion", Position = shot.Position + new Vector2(0, -5), Duration = 0.24f }); PlaySound("explosion2"); }
					break;
				}
				if (shot.Kind == "grenade")
				{
					obstacle.Alive = false; obstacle.Body.QueueFree();
					SpawnExplosion(obstacle.Bounds.GetCenter()); PlaySound("explosion");
					if (obstacle is MissileGuidanceEntity)
					{
						ExplodeMissile(); _awards.Add(new AwardPointsEntity(1000)); PlaySound("points");
					}
					else _points += 150;
				}
				else { _effects.Add(new Effect { Kind = "blaster_explosion", Position = shot.Position + new Vector2(0, -5), Duration = 0.24f }); PlaySound("explosion2"); }
				break;
			}
			if (remove) _shots.RemoveAt(i);
		}
	}

	private void UpdateMissile()
	{
		if (_missile == null && !_player.IsDead && _obstacles.Exists(obstacle => obstacle is MissileGuidanceEntity && obstacle.Alive))
		{
			_missile = new MissileEntity();
			_missileSpawnCount++;
		}
		if (_missile == null) return;
		_missile.Update(_player.Position.Y - 32);
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		if (_completeWindow == null && _missile.Bounds.Intersects(playerBounds))
		{
			if (_player.Kill()) _deathTime = 0;
			ExplodeMissile();
		}
		else if (_missile.Position.X < 0) _missile = null;
	}

	private void ExplodeMissile()
	{
		if (_missile == null) return;
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _missile.Position, Duration = 0.5f });
		_missile = null;
		PlaySound("explosion3");
	}

	private void UpdateLaunchers(float dt)
	{
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		foreach (DoubleLauncherEntity launcher in _launchers)
		{
			if (_completeWindow == null && launcher.Bounds.Intersects(playerBounds) && launcher.Capture()) { _awards.Add(new AwardPointsEntity(launcher.CapturePoints)); PlaySound("points"); }
			if (launcher.Update(_player.Position.X - 24, _random, out Vector2 bullet))
			{
				_shots.Add(new Shot { Kind = "double_launcher_bullet", Position = bullet, Velocity = new Vector2(-180, 0) });
				_launcherShotsFired++;
				PlaySound("rocket");
			}
		}
		UpdateAwards(dt);
	}

	private void UpdateAwards(float dt)
	{
		for (int i = _awards.Count - 1; i >= 0; i--)
		{
			_points += _awards[i].Update(dt);
			if (_awards[i].Remaining == 0) _awards.RemoveAt(i);
		}
	}

	private void UpdateExit()
	{
		if (_player.IsDead) return;
		Rect2 player = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		if (!_exits.Exists(exit => exit.Intersects(player))) return;
		_exits.Clear();
		int bravery = _player.IsArmored ? 0 : 10000;
		_completeWindow = new LevelCompleteWindowEntity { Name = "LevelCompleteWindow", ZIndex = 4000, Bravery = bravery, Lives = _lives };
		AddChild(_completeWindow);
		_player.Hide(); _player.SetPhysicsProcess(false);
		_awards.Add(new AwardPointsEntity(bravery + _lives * 1000, 250));
		PlaySound("points");
	}

	private void StartBonusScreen()
	{
		_bonusScreen = new BonusScreenEntity { Name = "BonusScreen", ZIndex = 4001, Finished = FinishBonusScreen };
		AddChild(_bonusScreen);
	}

	private void FinishBonusScreen(int points)
	{
		_points += points;
		_lives = Math.Min(_lives + 1, 9);
		_ammo = 99; _grenades = 10;
		_completeWindow?.QueueFree(); _completeWindow = null;
		_bonusScreen?.QueueFree(); _bonusScreen = null;
		_newLevelAfterBonus = true;
		Callable.From(AdvanceScreen).CallDeferred();
	}

	private void UpdateEggs()
	{
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _eggs.Count - 1; i >= 0; i--)
		{
			EggEntity egg = _eggs[i];
			egg.Update(_random, _groundRects);
			if (_completeWindow != null || !egg.Bounds.Intersects(playerBounds)) continue;
			if (_player.Kill()) _deathTime = 0;
			BurstEgg(i);
		}
	}

	private void BurstEgg(int index)
	{
		_effects.Add(new Effect { Kind = "blaster_explosion", Position = _eggs[index].Position, Duration = 0.35f });
		_eggs.RemoveAt(index);
		_points += 50;
		PlaySound("burst");
	}

	private void UpdateBubbles(float dt)
	{
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_bubbleCreator?.Update(dt, playerTopLeft, _player.IsDead) == true)
			_bubbles.Add(new BubbleEntity(playerTopLeft, _random, _bubbleCreator.Behavior));
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _bubbles.Count - 1; i >= 0; i--)
		{
			BubbleEntity bubble = _bubbles[i];
			bubble.Update(_random);
			if (_completeWindow == null && bubble.Bounds.Intersects(playerBounds))
			{
				if (_player.Kill()) _deathTime = 0;
				BurstBubble(i);
			}
			else if (bubble.Position.X < 0) _bubbles.RemoveAt(i);
		}
	}

	private void UpdateInterceptors(float dt)
	{
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_interceptorCreator?.Update(dt, playerTopLeft, _player.IsDead) == true)
			_interceptors.Add(new InterceptorEntity(playerTopLeft, _random, _interceptorCreator.Behavior));
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _interceptors.Count - 1; i >= 0; i--)
		{
			InterceptorEntity interceptor = _interceptors[i];
			interceptor.Update(_random);
			if (_completeWindow == null && interceptor.Bounds.Intersects(playerBounds))
			{
				if (_player.Kill()) _deathTime = 0;
				BurstInterceptor(i);
			}
			else if (interceptor.Position.X < 0) _interceptors.RemoveAt(i);
		}
	}

	private void BurstInterceptor(int index)
	{
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _interceptors[index].Position, Duration = 0.5f });
		_interceptors.RemoveAt(index);
		_points += 150;
		PlaySound("burst");
	}

	private void UpdateJellyfish(float dt)
	{
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_jellyfishCreator?.Update(dt, playerTopLeft, _player.IsDead) == true)
			_jellyfish.Add(new JellyfishEntity(playerTopLeft, _random, _jellyfishCreator.Behavior));
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _jellyfish.Count - 1; i >= 0; i--)
		{
			JellyfishEntity jellyfish = _jellyfish[i];
			jellyfish.Update();
			if (_completeWindow == null && jellyfish.Bounds.Intersects(playerBounds))
			{
				if (_player.Kill()) _deathTime = 0;
				BurstJellyfish(i);
			}
			else if (jellyfish.Position.X < 0) _jellyfish.RemoveAt(i);
		}
	}

	private void BurstJellyfish(int index)
	{
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _jellyfish[index].Position, Duration = 0.5f });
		_jellyfish.RemoveAt(index);
		_points += 150;
		PlaySound("burst");
	}

	private void UpdateHarbringers(float dt)
	{
		if (_player.IsDead) _harbringerTimer = 0;
		_harbringerTimer += dt;
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_harbringerTimer > 40f)
		{
			_harbringerTimer = 0;
			if (playerTopLeft.X <= 320) _harbringers.Add(new HarbringerEntity(playerTopLeft));
		}
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _harbringers.Count - 1; i >= 0; i--)
		{
			HarbringerEntity enemy = _harbringers[i]; enemy.Update();
			if (_completeWindow == null && enemy.Bounds.Intersects(playerBounds))
			{
				if (_player.Kill()) _deathTime = 0;
				_effects.Add(new Effect { Kind = "circular_explosion", Position = enemy.Position, Duration = 0.5f });
				_harbringers.RemoveAt(i); PlaySound("burst");
			}
			else if (enemy.Position.X < 0) _harbringers.RemoveAt(i);
		}
	}

	private void UpdateFlyingEnemies(float dt)
	{
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_flyingCreator?.Update(dt, playerTopLeft, _player.IsDead) == true)
			_flyingEnemies.Add(new FlyingEnemyEntity(_flyingCreator.Kind, _flyingCreator.Behavior, playerTopLeft, _random));
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _flyingEnemies.Count - 1; i >= 0; i--)
		{
			FlyingEnemyEntity enemy = _flyingEnemies[i]; enemy.Update(_random);
			if (_completeWindow == null && enemy.Bounds.Intersects(playerBounds)) { if (_player.Kill()) _deathTime = 0; BurstFlyingEnemy(i); }
			else if (enemy.Position.X < 0) _flyingEnemies.RemoveAt(i);
		}
	}

	private void BurstFlyingEnemy(int index)
	{
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _flyingEnemies[index].Position, Duration = 0.5f });
		_flyingEnemies.RemoveAt(index); _points += 150; PlaySound("burst");
	}

	private void UpdateLice(float dt)
	{
		Vector2 playerTopLeft = _player.Position - new Vector2(24, 32);
		if (_louseCreator?.Update(dt, playerTopLeft, _player.IsDead) == true)
			_lice.Add(new LouseEntity(playerTopLeft, _random, _louseCreator.Behavior));
		Rect2 playerBounds = new(_player.Position - new Vector2(23, 31), new Vector2(46, 63));
		for (int i = _lice.Count - 1; i >= 0; i--)
		{
			LouseEntity louse = _lice[i]; louse.Update();
			if (_completeWindow == null && louse.Bounds.Intersects(playerBounds))
			{
				if (_player.Kill()) _deathTime = 0;
				BurstLouse(i);
			}
			else if (louse.Position.X < 0) _lice.RemoveAt(i);
		}
	}

	private void BurstLouse(int index)
	{
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _lice[index].Position, Duration = 0.5f });
		_lice.RemoveAt(index); _points += 150; PlaySound("burst");
	}

	private void BurstBubble(int index)
	{
		_effects.Add(new Effect { Kind = "circular_explosion", Position = _bubbles[index].Position, Duration = 0.5f });
		_bubbles.RemoveAt(index);
		_points += 150;
		PlaySound("burst");
	}

	private void UpdateMines()
	{
		Vector2 topLeft = _player.Position - new Vector2(23, _player.IsDucking ? 20 : 31);
		Rect2 playerBounds = new(topLeft, new Vector2(46, _player.IsDucking ? 52 : 63));
		foreach (MineEntity mine in _mines)
		{
			if (_completeWindow != null || mine.Triggered || !mine.TriggerBounds.Intersects(playerBounds)) continue;
			mine.Trigger();
			if (!_player.IsArmored && _player.Kill()) _deathTime = 0;
			SpawnExplosion(mine.Position + new Vector2(16, 8), 30, 0);
			_mineFires.Add(new MineFireEntity(mine.Position));
			PlaySound("explosion");
		}
		foreach (MineFireEntity fire in _mineFires) fire.Update(_random);
	}

	private void SpawnExplosion(Vector2 position, int count = 50, int xOffset = 8)
	{
		for (int i = 0; i < count; i++)
		{
			float xSign = _random.RandiRange(0, 1) == 0 ? -1 : 1;
			float ySign = _random.RandiRange(0, 1) == 0 ? -1 : 1;
			_effects.Add(new Effect {
				Kind = "explosion_particle", Position = position - new Vector2(xOffset, 0), Frame = _random.RandiRange(0, 3), Flip = xSign > 0,
				Acceleration = new Vector2(_random.RandfRange(0.01f, 0.41f) * xSign, _random.RandfRange(0.01f, 0.41f) * ySign) * 3600,
				MaxVelocity = new Vector2(_random.RandfRange(3, 10) * xSign, _random.RandfRange(3, 10) * ySign) * 60
			});
		}
	}

	private void UpdateEffects(float dt)
	{
		for (int i = _effects.Count - 1; i >= 0; i--)
		{
			Effect effect = _effects[i];
			effect.Age += dt;
			if (effect.Kind == "explosion_particle")
			{
				effect.Velocity = new Vector2(Mathf.MoveToward(effect.Velocity.X, effect.MaxVelocity.X, Math.Abs(effect.Acceleration.X) * dt), Mathf.MoveToward(effect.Velocity.Y, effect.MaxVelocity.Y, Math.Abs(effect.Acceleration.Y) * dt));
				effect.Position += effect.Velocity * dt;
				if (effect.Position.X + 32 < 0 || effect.Position.X > 512 || effect.Position.Y + 16 < 0 || effect.Position.Y + 16 > 352) _effects.RemoveAt(i);
				continue;
			}
			effect.Position += effect.Velocity * dt;
			if (effect.Age >= effect.Duration) _effects.RemoveAt(i);
		}
	}

	private void PlaySound(string name)
	{
		if (!_audioChannels.TryGetValue(name, out List<AudioStreamPlayer>? channels))
		{
			int count = name switch { "shot2" or "explosion2" or "explosion3" or "rocket" => 3, "shot1" or "explosion" or "teleport" or "ding" => 2, _ => 1 };
			AudioStream stream = GD.Load<AudioStream>($"res://assets/sound/{name}.ogg");
			channels = new List<AudioStreamPlayer>(count);
			for (int i = 0; i < count; i++)
			{
				var channel = new AudioStreamPlayer { Stream = stream };
				AddChild(channel);
				channels.Add(channel);
			}
			_audioChannels[name] = channels;
		}
		AudioStreamPlayer available = channels.Find(channel => !channel.Playing) ?? channels[0];
		available.Play();
	}

	private void AddGroundCollision(int x, int y)
	{
		_groundRects.Add(new Rect2(x, y, 16, 16));
		var body = new StaticBody2D { Position = new Vector2(x + 8, y + 8) };
		body.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(16, 16) } });
		AddChild(body);
		_ground.Add(body);
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		foreach (var (x, y, gid) in _stars) DrawTile(x, y, gid);
		foreach (var (x, y, gid) in _tiles) DrawTile(x, y, gid);
	}

	private void DrawDynamicObjects(CanvasItem target)
	{
		foreach (Effect effect in _effects)
		{
			if (effect.Kind is not ("circular_explosion" or "explosion_particle")) DrawEffect(target, effect);
		}
		foreach (BubbleEntity bubble in _bubbles)
			DrawFrameOn(target, "bubble", (int)bubble.Position.X, (int)bubble.Position.Y, 32, 32, bubble.Frame);
		foreach (InterceptorEntity interceptor in _interceptors)
			DrawFrameOn(target, "interceptor", (int)interceptor.Position.X, (int)interceptor.Position.Y, 32, 32, interceptor.Frame);
		foreach (JellyfishEntity jellyfish in _jellyfish)
			DrawFrameOn(target, "jellyfish", (int)jellyfish.Position.X, (int)jellyfish.Position.Y, 32, 32, jellyfish.Frame);
		foreach (LouseEntity louse in _lice)
			DrawFrameOn(target, "louse", (int)louse.Position.X, (int)louse.Position.Y, 32, 32, louse.Frame);
		foreach (FlyingEnemyEntity enemy in _flyingEnemies)
			DrawFrameOn(target, enemy.Kind, (int)enemy.Position.X, (int)enemy.Position.Y, 32, 32, enemy.Frame);
		foreach (HarbringerEntity enemy in _harbringers)
			DrawFrameOn(target, "harbringer", (int)enemy.Position.X, (int)enemy.Position.Y, 128, 32, enemy.Frame);
		foreach (EggEntity egg in _eggs)
			if (!egg.HasOwnView) DrawFrameOn(target, "egg", (int)egg.Position.X, (int)egg.Position.Y, 16, 16, egg.Frame);
		if (_missile != null)
			DrawFrameOn(target, "missile", (int)_missile.Position.X, (int)_missile.Position.Y, 32, 32, _missile.Fast ? 1 : 0);
		foreach (Shot shot in _shots)
		{
			if (shot.Destroyed) continue;
			Texture2D texture = Texture(shot.Kind);
			if (shot.Kind == "grenade" && shot.Direction < 0)
			{
				target.DrawSetTransform(shot.Position + new Vector2(texture.GetWidth(), 0), 0, new Vector2(-1, 1));
				target.DrawTexture(texture, Vector2.Zero);
				target.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
			}
			else target.DrawTexture(texture, shot.Position);
		}
	}

	private void DrawEffects(CanvasItem target, string kind)
	{
		foreach (Effect effect in _effects) if (effect.Kind == kind) DrawEffect(target, effect);
	}

	private void DrawEffect(CanvasItem target, Effect effect)
	{
		int width = effect.Kind == "grenade_trace" ? 64 : effect.Kind is "explosion_particle" or "circular_explosion" ? 32 : 16;
		int height = effect.Kind == "grenade_trace" ? 48 : effect.Kind == "circular_explosion" ? 32 : 16;
		int frames = Texture(effect.Kind).GetWidth() / width;
		int frame = effect.Kind == "explosion_particle" ? effect.Frame : Mathf.Min((int)(effect.Age / effect.Duration * frames), frames - 1);
		Texture2D texture = Texture(effect.Kind);
		if (effect.Flip)
		{
			target.DrawSetTransform(effect.Position + new Vector2(width, 0), 0, new Vector2(-1, 1));
			target.DrawTextureRectRegion(texture, new Rect2(0, 0, width, height), new Rect2(frame * width, 0, width, height));
			target.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
		}
		else target.DrawTextureRectRegion(texture, new Rect2(effect.Position, new Vector2(width, height)), new Rect2(frame * width, 0, width, height));
	}


	private MapObjectView AddObjectView(LevelMap.MapObject obj)
	{
		var view = new MapObjectView { ObjectName = obj.Name, MapPosition = new Vector2I(obj.X, obj.Y), ZIndex = PlayerZ + obj.DrawOrder };
		view.DrawObject = target => DrawMapObject(target, obj.Name, obj.X, obj.Y, obj.Placement);
		AddChild(view);
		_objectViews.Add(view);
		return view;
	}

	private void DrawMapObject(CanvasItem target, string name, int x, int y, string placement)
	{
			if (!Engine.IsEditorHint() && name is ("turret" or "cocoon" or "radar" or "rocket" or "incubator" or "missile_guidance" or "waggon" or "combined_launcher_top" or "fungus" or "beam"))
				if (!_obstacles.Exists(o => o.Name == name && o.MapX == x && o.MapBottom == y && o.Alive)) return;
			if (name == "grenade_pack" && _grenadePackCollected) return;
			if (name == "ammo_pack" && _ammoPackCollected) return;
			switch (name)
			{
				case "ship_fire": DrawFrameOn(target, "ship_fire", x, y - 32, 16, 32, _shipFireFrames.GetValueOrDefault(new Vector2I(x, y))); break;
				case "light": DrawFrameOn(target, "light", x, y - 16, 16, 16, (placement == "ceiling" ? 8 : 0) + _lightFrame); break;
				case "square_light": DrawFrameOn(target, "square_light", x, y - 16, 16, 16, _lightFrame); break;
				case "discharge": DrawFrameOn(target, "discharge", x, y - 32, 64, 32, _dischargeFrames.GetValueOrDefault(new Vector2I(x, y))); break;
				case "vitorc": DrawFrameOn(target, "vitorc2", x, y - 64, 48, 64, 0); break;
				case "turret":
					DrawFrameOn(target, "turret_body", x + 32, y - 80, 64, 80, 0);
					var turret = _obstacles.Find(o => o is TurretEntity && o.MapX == x && o.MapBottom == y) as TurretEntity;
					DrawFrameOn(target, "turret_tube", x, y - 64, 32, 16, turret?.TubeFrame ?? 0);
					break;
				case "cocoon": DrawFrameOn(target, "cocoon", x, y - 128, 80, 128, 0); break;
				case "radar": DrawFrameOn(target, "radar", x, y - 128, 80, 128, 0); break;
				case "rocket": DrawFrameOn(target, "rocket", x, y - 96, 64, 96, 0); break;
				case "waggon": DrawFrameOn(target, "waggon", x, y - 80, 64, 80, 0); break;
				case "fungus": DrawFrameOn(target, "fungus", x, y - 64, 48, 64, 0); break;
				case "beam": DrawFrameOn(target, "beam", x, y - 256, 16, 256, TestBeamFrame()); break;
				case "double_launcher": DrawFrameOn(target, "double_launcher", x, y - 48, 64, 48, 0); break;
				case "combined_launcher_bottom": DrawFrameOn(target, "combined_launcher_bottom", x, y - 32, 96, 32, 0); break;
				case "combined_launcher_top":
					DrawFrameOn(target, "combined_launcher_top", x, y - 32, 80, 32, 0);
					var top = _obstacles.Find(o => o is CombinedLauncherTopEntity && o.MapX == x && o.MapBottom == y) as CombinedLauncherTopEntity;
					DrawFrameOn(target, "turret_tube", x - 16, y - 32, 32, 16, top?.TubeFrame ?? 0);
					break;
				case "mine":
					if (Engine.IsEditorHint() || _mines.Exists(mine => mine.Position.X == x && mine.Position.Y == y - 16 && !mine.Triggered))
						DrawFrameOn(target, "mine", x, y - 16, 32, 16, 0);
					else
					{
						MineFireEntity? fire = _mineFires.Find(item => item.Position == new Vector2(x, y - 16));
						if (fire != null) DrawFrameOn(target, "mine_fire", x, y - 16, 32, 16, fire.Frame);
					}
					break;
				case "incubator":
					DrawFrameOn(target, "incubator", x, y - 96, 64, 96, 0);
					if (Engine.IsEditorHint()) DrawFrameOn(target, "egg", x + 20, y - 76, 16, 16, 0);
					break;
				case "missile_guidance": DrawFrameOn(target, "missile_guidance", x, y - 64, 64, 64, 0); break;
				case "grenade_pack": DrawFrameOn(target, "grenade_pack", x, y - 32, 32, 32, 0); break;
				case "ammo_pack": DrawFrameOn(target, "ammo_pack", x, y - 32, 32, 32, 0); break;
				case "piston":
					float rise = _pistons.Find(item => item.X == x && item.Bottom == y).Piston?.Rise ?? 0;
					if (rise > 0) target.DrawTextureRectRegion(Texture("piston"), new Rect2(x, y - rise, 48, rise), new Rect2(0, 0, 48, rise));
					break;
				default:
					if (Engine.IsEditorHint() && _objectGids.TryGetValue((name, x, y), out uint gid) && gid != 0)
						DrawTileOn(target, x, y - 16, gid);
					break;
			}
		}

	private void DrawTileOn(CanvasItem target, int x, int y, uint gid)
	{
		TileSetInfo? chosen = null;
		foreach (TileSetInfo set in _tileSets) if (set.FirstGid <= gid) chosen = set;
		if (chosen == null) return;
		int index = (int)(gid - chosen.FirstGid);
		int columns = chosen.Texture.GetWidth() / chosen.Width;
		var source = new Rect2(index % columns * chosen.Width, index / columns * chosen.Height, chosen.Width, chosen.Height);
		target.DrawTextureRectRegion(chosen.Texture, new Rect2(x, y + 16 - chosen.Height, chosen.Width, chosen.Height), source);
	}

	private void DrawFrameOn(CanvasItem target, string name, int x, int y, int width, int height, int frame)
	{
		Texture2D texture = Texture(name);
		int columns = texture.GetWidth() / width;
		target.DrawTextureRectRegion(texture, new Rect2(x, y, width, height), new Rect2(frame % columns * width, frame / columns * height, width, height));
	}

	private void DrawTile(int x, int y, uint gid)
	{
		TileSetInfo? chosen = null;
		foreach (TileSetInfo set in _tileSets)
			if (set.FirstGid <= gid) chosen = set;
		if (chosen == null) return;
		int index = (int)(gid - chosen.FirstGid);
		int columns = chosen.Texture.GetWidth() / chosen.Width;
		var source = new Rect2(index % columns * chosen.Width, index / columns * chosen.Height, chosen.Width, chosen.Height);
		DrawTextureRectRegion(chosen.Texture, new Rect2(x, y + 16 - chosen.Height, chosen.Width, chosen.Height), source);
	}

	private void DrawHud(CanvasItem target)
	{
		target.DrawRect(new Rect2(0, 352, 512, 32), Colors.Black);
		DrawTextOn(target, "cyan", "AMMO", 0, 352); DrawTextOn(target, "white", _ammo.ToString("D2"), 16, 368);
		DrawTextOn(target, "yellow", "GRENADES", 80, 352); DrawTextOn(target, "purple", _grenades.ToString("D2"), 128, 368);
		DrawTextOn(target, "green", "POINTS", 224, 352); DrawTextOn(target, "cyan", _points.ToString("D6"), 224, 368);
		DrawTextOn(target, "white", "LIVES", 336, 352); DrawTextOn(target, "yellow", _lives.ToString(), 368, 368);
		DrawTextOn(target, "purple", "ZONES", 432, 352); DrawTextOn(target, "yellow", _zone.ToString("D3"), 448, 368);
	}

	private void DrawTextOn(CanvasItem target, string font, string text, int x, int y)
	{
		foreach (char c in text) { DrawFrameOn(target, "font_" + font, x, y, 16, 16, c - 32); x += 16; }
	}
}
