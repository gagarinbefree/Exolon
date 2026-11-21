using Godot;

// The original font sheets contain the final multicolour pixels for each glyph.
[Tool]
public partial class BitmapText : Control
{
	private string _text = "";
	private Texture2D? _atlas;
	private bool _centered;

	[Export] public string Text
	{
		get => _text;
		set { if (_text != value) { _text = value; QueueRedraw(); } }
	}

	[Export] public Texture2D? Atlas
	{
		get => _atlas;
		set { _atlas = value; QueueRedraw(); }
	}

	[Export] public bool Centered
	{
		get => _centered;
		set { _centered = value; QueueRedraw(); }
	}

	protected virtual Texture2D? FontTexture => Atlas;

	public override void _Draw()
	{
		Texture2D? font = FontTexture;
		if (font == null) return;
		const int cell = 16;
		int columns = font.GetWidth() / cell;
		if (columns == 0) return;
		float x = Centered ? Mathf.Floor((Size.X - Text.Length * cell) / 2) : 0;
		foreach (char glyph in Text)
		{
			int index = glyph - ' ';
			if (index >= 0 && index / columns < font.GetHeight() / cell)
				DrawTextureRectRegion(font, new Rect2(x, 0, cell, cell),
					new Rect2(index % columns * cell, index / columns * cell, cell, cell));
			x += cell;
		}
	}
}
