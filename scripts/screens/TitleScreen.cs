using Godot;

public partial class TitleScreen : Node2D
{
	private AudioStreamPlayer _audio = null!;

	public override void _Ready()
	{
		_audio = new AudioStreamPlayer { Stream = GD.Load<AudioStream>("res://assets/sound/theme.ogg") };
		AddChild(_audio);
		_audio.Play();
		QueueRedraw();
	}

	public override void _ExitTree()
	{
		if (IsInstanceValid(_audio)) { _audio.Stop(); _audio.Stream = null; }
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("fire")) GetTree().ChangeSceneToFile("res://scenes/screens/first_screen.tscn");
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/title.png"), new Vector2(128, 0));
		DrawText("green", "BY  RAFFAELE CECCO", 112, 112);
		DrawText("cyan", "PRESS      TO PLAY", 112, 208);
		DrawText("purple", "      FIRE        ", 112, 208);
		DrawText("red", "EXOLON COPYRIGHT 1987 HEWSON", 32, 368);
	}

	private void DrawText(string color, string text, int x, int y)
	{
		Texture2D font = GD.Load<Texture2D>($"res://assets/images/font_{color}.png");
		int columns = font.GetWidth() / 16;
		foreach (char c in text)
		{
			int index = c - 32;
			DrawTextureRectRegion(font, new Rect2(x, y, 16, 16), new Rect2(index % columns * 16, index / columns * 16, 16, 16));
			x += 16;
		}
	}
}
