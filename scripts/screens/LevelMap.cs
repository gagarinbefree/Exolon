using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Reads the same editable Godot scene that is opened in the map editor.
public sealed class LevelMap
{
	public sealed record Tileset(string Name, uint FirstGid, int Width, int Height);
	public sealed record Tile(string Layer, int X, int Y, uint Gid, bool Foreground, int DrawOrder);
	public sealed record MapObject(string Name, int X, int Y, string Placement, uint Gid, int Width, int Height, int DrawOrder, string Behavior, float Delay, int FireDurationMin, int FireDurationMax);
	public string NextLevel { get; private set; } = "";
	public int DynamicDrawOrder { get; private set; }
	public readonly List<Tileset> Tilesets = new();
	public readonly List<Tile> Tiles = new();
	public readonly List<MapObject> Objects = new();

	public static bool Exists(string name) => CanEnter(name) && ResourceLoader.Exists($"res://scenes/maps/{name}.tscn");
	public static bool CanEnter(string name)
	{
		if (name.Length != 6 || name[0] != 'L' || name[3] != 'S' || !int.TryParse(name.AsSpan(1, 2), out int level) || !int.TryParse(name.AsSpan(4, 2), out int screen)) return false;
		return level is >= 1 and <= 3 && screen is >= 1 and <= 25;
	}

	public static LevelMap Load(string name)
	{
		if (!CanEnter(name)) throw new InvalidOperationException($"Invalid map name: {name}");
		PackedScene packed = GD.Load<PackedScene>($"res://scenes/maps/{name}.tscn")
			?? throw new InvalidOperationException($"Missing editable map: {name}");
		Node map = packed.Instantiate();
		try
		{
			var result = new LevelMap { NextLevel = map.Get("next_level").AsString() };
			var columns = new Dictionary<int, int>();
			foreach (Variant item in map.Get("tilesets").AsGodotArray())
			{
				var set = item.AsGodotDictionary();
				int first = set["firstgid"].AsInt32();
				int width = set["width"].AsInt32();
				result.Tilesets.Add(new(set["name"].AsString(), (uint)first, width, set["height"].AsInt32()));
				columns[first] = set["image_width"].AsInt32() / width;
			}
			var layers = map.GetChildren().OfType<Node2D>().ToArray();
			int playerOrder = 0;
			for (int i = 0; i < layers.Length; i++)
			{
				int index = 0;
				foreach (Node obj in layers[i].GetChildren())
				{
					if (obj.Get("object_type").AsString() == "vitorc") playerOrder = i * 128 + 127 - index;
					index++;
				}
			}
			bool afterObjects = false;
			for (int i = 0; i < layers.Length; i++)
			{
				Node2D layer = layers[i];
				string layerName = layer.GetMeta("map_layer_name", layer.Name.ToString()).AsString();
				string kind = layer.GetMeta("map_layer_kind", "").AsString();
				if (kind == "objectgroup")
				{
					int index = 0;
					foreach (Node2D obj in layer.GetChildren().OfType<Node2D>())
					{
						string type = obj.Get("object_type").AsString();
						if (type == "vitorc") { afterObjects = true; result.DynamicDrawOrder = i * 128 - playerOrder; }
						Vector2I size = obj.Get("size").AsVector2I();
						result.Objects.Add(new(type, (int)obj.Position.X, (int)obj.Position.Y, obj.Get("placement").AsString(), (uint)obj.Get("gid").AsInt32(), size.X, size.Y,
							i * 128 + 127 - index++ - playerOrder, obj.Get("behavior").AsString(), obj.Get("delay").AsSingle(), obj.Get("fire_duration_min").AsInt32(), obj.Get("fire_duration_max").AsInt32()));
					}
				}
				else if (layer is TileMapLayer tileLayer)
				{
					foreach (Vector2I cell in tileLayer.GetUsedCells())
					{
						int source = tileLayer.GetCellSourceId(cell);
						Vector2I atlas = tileLayer.GetCellAtlasCoords(cell);
						if (!columns.TryGetValue(source, out int cols)) throw new InvalidOperationException($"Unknown tileset {source} in {name}");
						uint gid = (uint)(source + atlas.Y * cols + atlas.X);
						result.Tiles.Add(new(layerName, cell.X * 16, cell.Y * 16, gid, afterObjects, i * 128 - playerOrder));
					}
				}
			}
			return result;
		}
		finally { map.Free(); }
	}
}
