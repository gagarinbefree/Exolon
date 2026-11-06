using Godot;

// Same overlay and bitmap font coordinates as melonJS GameOverWindow.
public partial class GameOverWindow : Node2D
{
	public override void _Draw()
	{
		DrawRect(new Rect2(160, 144, 176, 48), new Color(0xef / 255f, 0, 0));
		Texture2D font = GD.Load<Texture2D>("res://assets/images/font_yellow.png");
		int columns = font.GetWidth() / 16;
		int x = 176;
		foreach (char c in "GAME OVER")
		{
			int index = c - 32;
			DrawTextureRectRegion(font, new Rect2(x, 160, 16, 16), new Rect2(index % columns * 16, index / columns * 16, 16, 16));
			x += 16;
		}
	}
}
