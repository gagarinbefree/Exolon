using Godot;
using System;
using System.Reflection;

// Local test harness only. The normal game never instantiates this node.
public partial class UnlimitedResources : Node
{
	private static readonly FieldInfo Lives = Field("_lives");
	private static readonly FieldInfo Ammo = Field("_ammo");
	private static readonly FieldInfo Grenades = Field("_grenades");
	public Node? World { get; set; }
	public int RefillTicks { get; private set; }
	public void Attach(Node world) => World = world;

	private static FieldInfo Field(string name) => typeof(FirstScreen).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
		?? throw new InvalidOperationException("Missing test resource field " + name);

	public override void _Ready()
	{
		if (OS.GetEnvironment("EXOLON_TEST_UNLIMITED") != "1" || string.IsNullOrWhiteSpace(OS.GetEnvironment("EXOLON_SAVE_PATH")))
			throw new InvalidOperationException("Unlimited resources require explicit test mode and an isolated save path.");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (World is not FirstScreen world || !IsInstanceValid(world)) return;
		Lives.SetValue(world, 9);
		Ammo.SetValue(world, 99);
		Grenades.SetValue(world, 10);
		RefillTicks++;
	}
}
