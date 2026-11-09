using Godot;

// Shown after the original four-second GAME OVER overlay.
public partial class GameOverMenu : Node2D
{
	private bool _hasSave;
	private int _selection;

	public override void _Ready()
	{
		_hasSave = AutoSave.HasSave();
		QueueRedraw();
	}

	public override void _PhysicsProcess(double delta)
	{
		bool fire = Input.IsActionJustPressed("fire");
		if (!fire && _hasSave && (Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down")))
		{
			_selection = 1 - _selection;
			QueueRedraw();
		}
		if (!fire) return;
		if (_selection == 0) AutoSave.StartNew();
		else if (!AutoSave.PrepareContinue()) { _hasSave = false; _selection = 0; QueueRedraw(); return; }
		GetTree().ChangeSceneToFile("res://scenes/screens/first_screen.tscn");
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/title.png"), new Vector2(128, 0));
		DrawText("red", "GAME OVER", 128);
		DrawOption("START AGAIN", _hasSave ? 200 : 220, _selection == 0);
		if (_hasSave) DrawOption("CONTINUE", 240, _selection == 1);
		DrawText("green", "UP DOWN + FIRE", 304);
	}

	private void DrawOption(string text, int y, bool selected)
	{
		int x = (512 - text.Length * 16) / 2;
		if (selected) DrawTextureRectRegion(GD.Load<Texture2D>("res://assets/images/egg.png"), new Rect2(x - 24, y, 16, 16), new Rect2(0, 0, 16, 16));
		DrawText(selected ? "purple" : "cyan", text, y);
	}

	private void DrawText(string color, string text, int y)
	{
		Texture2D font = GD.Load<Texture2D>($"res://assets/images/font_{color}.png");
		int columns = font.GetWidth() / 16;
		int x = (512 - text.Length * 16) / 2;
		foreach (char c in text)
		{
			int index = c - 32;
			DrawTextureRectRegion(font, new Rect2(x, y, 16, 16), new Rect2(index % columns * 16, index / columns * 16, 16, 16));
			x += 16;
		}
	}
}
