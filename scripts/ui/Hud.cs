using Godot;

[Tool]
public partial class Hud : Node2D
{
	private BitmapText _ammo = null!, _grenades = null!, _points = null!, _lives = null!, _zones = null!;
	private (int, int, int, int, int)? _last;

	public override void _Ready()
	{
		_ammo = GetNode<BitmapText>("Ammo/Value");
		_grenades = GetNode<BitmapText>("Grenades/Value");
		_points = GetNode<BitmapText>("Points/Value");
		_lives = GetNode<BitmapText>("Lives/Value");
		_zones = GetNode<BitmapText>("Zones/Value");
	}

	public void SetValues(int ammo, int grenades, int points, int lives, int zones)
	{
		if (_last == (ammo, grenades, points, lives, zones)) return;
		_last = (ammo, grenades, points, lives, zones);
		_ammo.Text = ammo.ToString("D2");
		_grenades.Text = grenades.ToString("D2");
		_points.Text = points.ToString("D6");
		_lives.Text = lives.ToString();
		_zones.Text = zones.ToString("D3");
	}
}
