using Godot;
using System;

public partial class BonusScreenEntity : Node2D
{
	private static readonly int[] Values = { 0, 1000, 0, 3000, 0, 5000, 0, 7000 };
	private int _pointer;
	private int _ticks;
	private bool _fireHeld;
	private bool _stopped;
	private float _stopTime;
	public Action<int>? Finished;
	public override void _Ready() => _fireHeld = Input.IsActionPressed("fire");
	public override void _PhysicsProcess(double delta)
	{
		if (_stopped)
		{
			_stopTime += (float)delta;
			if (_stopTime >= 1f) { SetPhysicsProcess(false); Finished?.Invoke(Values[_pointer]); }
			return;
		}
		if (++_ticks > 1) { _ticks = 0; _pointer = (_pointer + 1) % Values.Length; }
		bool pressed = Input.IsActionPressed("fire");
		if (pressed && !_fireHeld) _stopped = true;
		_fireHeld = pressed;
		QueueRedraw();
	}
	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 512, 384), Colors.Black);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/bonus_screen.png"), Vector2.Zero);
		DrawTexture(GD.Load<Texture2D>("res://assets/images/title.png"), new Vector2(128, 48));
		DrawText("white", "EXOLON BONUS SCREEN!", 96, 128);
		DrawText("green", "PRESS FIRE TO STOP POINTER", 48, 160);
		for (int i = 0; i < Values.Length; i++)
		{
			int y = 208 + i * 16;
			DrawText(i % 2 == 0 ? "yellow" : "cyan", Values[i].ToString("D5") + " POINTS", 144, y);
			if (_pointer == i) DrawTexture(GD.Load<Texture2D>("res://assets/images/arrow.png"), new Vector2(352, y));
		}
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
