using Godot;

[Tool]
public partial class MenuOption : BitmapText
{
	private bool _selected;
	private Texture2D? _selectedAtlas;
	private Sprite2D? _cursor;

	[Export] public Texture2D? SelectedAtlas
	{
		get => _selectedAtlas;
		set { _selectedAtlas = value; QueueRedraw(); }
	}

	[Export] public bool Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			if (IsInstanceValid(_cursor)) _cursor!.Visible = value;
			QueueRedraw();
		}
	}

	protected override Texture2D? FontTexture => Selected ? SelectedAtlas : Atlas;

	public override void _Ready()
	{
		_cursor = GetNode<Sprite2D>("Cursor");
		_cursor.Visible = Selected;
	}
}
