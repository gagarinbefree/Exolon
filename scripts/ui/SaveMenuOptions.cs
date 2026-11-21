using Godot;

// Continue has its own row. The other two actions move together if the slot is empty.
public partial class SaveMenuOptions : Node2D
{
	private MenuOption _resume = null!;
	private Node2D _actions = null!;
	private MenuOption _first = null!;
	private MenuOption _second = null!;
	private Vector2 _withSave;
	private Marker2D _withoutSave = null!;

	public override void _Ready()
	{
		_resume = GetNode<MenuOption>("Continue");
		_actions = GetNode<Node2D>("Actions");
		_first = _actions.GetChild<MenuOption>(0);
		_second = _actions.GetChild<MenuOption>(1);
		_withSave = _actions.Position;
		_withoutSave = GetNode<Marker2D>("ActionsWithoutSave");
	}

	public void ShowSelection(bool hasSave, int index)
	{
		_resume.Visible = hasSave;
		_actions.Position = hasSave ? _withSave : _withoutSave.Position;
		_resume.Selected = hasSave && index == 0;
		_first.Selected = index == (hasSave ? 1 : 0);
		_second.Selected = index == (hasSave ? 2 : 1);
	}
}
