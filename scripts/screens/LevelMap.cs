using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;

// Data read from the original melonJS TMX maps. PlayScreen owns the game state;
// the map supplies the layers, object definitions and link to the next screen.
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
	private static int Int(XElement element, string name) => (int?)element.Attribute(name) ?? 0;
	private static string Property(XElement element, string name, string fallback = "") =>
		(string?)element.Element("properties")?.Elements("property").FirstOrDefault(p => (string?)p.Attribute("name") == name)?.Attribute("value") ?? fallback;
	private static float Delay(XElement element) => float.TryParse(Property(element, "delay"), NumberStyles.Float, CultureInfo.InvariantCulture, out float delay) ? delay : 0;
	private static int IntProperty(XElement element, string name, int fallback) => int.TryParse(Property(element, name), out int value) && value > 0 ? value : fallback;

	public static bool Exists(string name) => !string.IsNullOrEmpty(name) && Godot.FileAccess.FileExists($"res://assets/maps/{name}.tmx");
	public static bool CanEnter(string name)
	{
		if (name.Length != 6 || name[0] != 'L' || name[3] != 'S' || !int.TryParse(name.AsSpan(1, 2), out int level) || !int.TryParse(name.AsSpan(4, 2), out int screen)) return false;
		return level is >= 1 and <= 3 && screen is >= 1 and <= 25;
	}

	public static LevelMap Load(string name)
	{
		XElement map = XDocument.Parse(Godot.FileAccess.GetFileAsString($"res://assets/maps/{name}.tmx")).Root
			?? throw new InvalidOperationException($"Map {name} has no root element");
		var result = new LevelMap();
		foreach (XElement property in map.Element("properties")?.Elements("property") ?? Array.Empty<XElement>())
			if ((string?)property.Attribute("name") == "nextLevel") result.NextLevel = (string?)property.Attribute("value") ?? "";
		foreach (XElement set in map.Elements("tileset"))
			result.Tilesets.Add(new((string?)set.Attribute("name") ?? throw new InvalidOperationException("Missing tileset name"), (uint)Int(set, "firstgid"), Int(set, "tilewidth"), Int(set, "tileheight")));
		var orders = new Dictionary<XElement, int>();
		int layerOrder = 0;
		int playerOrder = 0;
		foreach (XElement layer in map.Elements())
		{
			if (layer.Name != "layer" && layer.Name != "objectgroup" && layer.Name != "imagelayer") continue;
			orders[layer] = layerOrder * 128;
			int index = 0;
			foreach (XElement obj in layer.Elements("object"))
			{
				orders[obj] = layerOrder * 128 + 127 - index++;
				if ((string?)obj.Attribute("name") == "vitorc") playerOrder = orders[obj];
			}
			layerOrder++;
		}
		foreach (XElement layer in map.Elements("objectgroup"))
			if (layer.Elements("object").Any(obj => (string?)obj.Attribute("name") == "vitorc"))
				result.DynamicDrawOrder = orders[layer] - playerOrder;
		bool afterObjects = false;
		foreach (XElement layer in map.Elements())
		{
			if (layer.Name == "objectgroup")
			{
				// Some maps have an earlier empty or background object group.
				// Tiles become foreground only after the player's object layer.
				foreach (XElement obj in layer.Elements("object"))
					if ((string?)obj.Attribute("name") == "vitorc") afterObjects = true;
				continue;
			}
			if (layer.Name != "layer") continue;
			string layerName = (string?)layer.Attribute("name") ?? "";
			byte[] data = Convert.FromBase64String(layer.Element("data")?.Value ?? "");
			int width = Int(layer, "width");
			for (int i = 0; i < data.Length / 4; i++)
			{
				uint gid = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i * 4, 4));
				if (gid != 0) result.Tiles.Add(new(layerName, i % width * 16, i / width * 16, gid, afterObjects, orders[layer] - playerOrder));
			}
		}
		foreach (XElement obj in map.Descendants("object"))
			result.Objects.Add(new((string?)obj.Attribute("name") ?? "", Int(obj, "x"), Int(obj, "y"), Property(obj, "placement", "floor"), (uint)Int(obj, "gid"), Int(obj, "width"), Int(obj, "height"), orders[obj] - playerOrder, Property(obj, "behavior"), Delay(obj), IntProperty(obj, "fireDurationMin", 20), IntProperty(obj, "fireDurationMax", 160)));
		return result;
	}
}
