using Godot;

// Shown after the original four-second GAME OVER overlay.
public partial class GameOverMenu : Node2D
{
	private bool _hasSave;
	private int _selection;
	private SaveMenuOptions _options = null!;

	public override void _Ready()
	{
		_options = GetNode<SaveMenuOptions>("Options");
		_hasSave = AutoSave.HasSave();
		_options.ShowSelection(_hasSave, _selection);
	}

	public override void _PhysicsProcess(double delta)
	{
		bool fire = Input.IsActionJustPressed("fire");
		if (!fire && (Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down")))
		{
			int count = _hasSave ? 3 : 2;
			_selection = (_selection + (Input.IsActionJustPressed("ui_up") ? -1 : 1) + count) % count;
			_options.ShowSelection(_hasSave, _selection);
		}
		if (!fire) return;
		if (_selection == (_hasSave ? 2 : 1))
		{
			GetTree().ChangeSceneToFile("res://scenes/screens/title_screen.tscn");
			return;
		}
		if (_hasSave && _selection == 0)
		{
			if (!AutoSave.PrepareContinue()) { _hasSave = false; _selection = 0; _options.ShowSelection(_hasSave, _selection); return; }
		}
		else AutoSave.StartNew();
		GetTree().ChangeSceneToFile("res://scenes/screens/first_screen.tscn");
	}
}
