using System;
using Godot;

// Individual canvas items let TMX object groups retain their original draw order.
[Tool]
public partial class MapObjectView : Node2D
{
	public string ObjectName = "";
	public Vector2I MapPosition;
	public Action<CanvasItem>? DrawObject;
	public override void _Process(double delta) => QueueRedraw();
	public override void _Draw() => DrawObject?.Invoke(this);
}
