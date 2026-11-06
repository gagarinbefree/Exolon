using Godot;

// Invisible TMX trigger used to switch the player's outfit.
public sealed class CapsuleEntity
{
	public Rect2 Bounds { get; }
	public CapsuleEntity(int x, int y, int width, int height) => Bounds = new Rect2(x, y, width, height);
}
