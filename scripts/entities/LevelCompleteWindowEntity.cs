using Godot;

public partial class LevelCompleteWindowEntity : Node2D
{
	public int Bravery;
	public int Lives;
	public override void _Draw()
	{
		DrawTexture(GD.Load<Texture2D>("res://assets/images/level_complete_window.png"), new Vector2(112, 48));
		DrawText("purple", "BRAVERY BONUS", 160, 96);
		DrawText("white", Bravery.ToString("D5"), 224, 128);
		DrawText("green", "LIVES BONUS", 176, 160);
		DrawText("yellow", $"{Lives} X 1000", 208, 192);
		DrawText("cyan", "PRESS FIRE TO", 160, 224);
		DrawText("cyan", "RESUME PLAY", 176, 256);
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
