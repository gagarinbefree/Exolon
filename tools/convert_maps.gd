extends SceneTree

const MAP_SCRIPT = preload("res://scripts/maps/map_data.gd")
const OBJECT_SCRIPT = preload("res://scripts/maps/map_object.gd")

func _initialize() -> void:
	call_deferred("convert")

func find_set(sets: Array, gid: int) -> Dictionary:
	var result := {}
	for item: Dictionary in sets:
		if int(item.firstgid) <= gid and (result.is_empty() or int(item.firstgid) > int(result.firstgid)):
			result = item
	return result

func convert() -> void:
	var input := FileAccess.open("res://artifacts/map_conversion.json", FileAccess.READ)
	if input == null:
		push_error("Generate artifacts/map_conversion.json first")
		quit(1)
		return
	var maps: Array = JSON.parse_string(input.get_as_text())
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://scenes/maps"))
	for entry: Dictionary in maps:
		var map_root := Node2D.new()
		map_root.name = entry.id
		map_root.set_script(MAP_SCRIPT)
		map_root.set("next_level", entry.next_level)
		var sets: Array[Dictionary] = []
		for item: Dictionary in entry.tilesets: sets.append(item)
		map_root.set("tilesets", sets)
		var tile_set := TileSet.new()
		tile_set.tile_size = Vector2i(16, 16)
		var used := {}
		for layer: Dictionary in entry.layers:
			if layer.kind == "layer":
				for cell: Array in layer.cells:
					var info := find_set(sets, int(cell[2]))
					if not info.is_empty(): used[int(info.firstgid)] = info
		for first_gid in used:
			var info: Dictionary = used[first_gid]
			var texture: Texture2D = load("res://assets/images/%s.png" % info.name)
			if texture == null:
				push_error("Missing texture: " + str(info.name))
				quit(1)
				return
			var atlas := TileSetAtlasSource.new()
			atlas.texture = texture
			atlas.texture_region_size = Vector2i(info.width, info.height)
			for y in range(texture.get_height() / int(info.height)):
				for x in range(texture.get_width() / int(info.width)):
					atlas.create_tile(Vector2i(x, y))
					atlas.get_tile_data(Vector2i(x, y), 0).texture_origin = Vector2i((16 - int(info.width)) / 2, (int(info.height) - 16) / 2)
			tile_set.add_source(atlas, first_gid)
		var index := 0
		for layer: Dictionary in entry.layers:
			var child: Node2D
			if layer.kind == "layer":
				var tile_layer := TileMapLayer.new()
				tile_layer.tile_set = tile_set
				tile_layer.collision_enabled = false
				for cell: Array in layer.cells:
					var info := find_set(sets, int(cell[2]))
					if info.is_empty(): continue
					var cols: int = int(info.image_width) / int(info.width)
					var frame: int = int(cell[2]) - int(info.firstgid)
					tile_layer.set_cell(Vector2i(cell[0], cell[1]), int(info.firstgid), Vector2i(frame % cols, frame / cols))
				child = tile_layer
			else:
				child = Node2D.new()
			child.name = "%02d_%s" % [index, str(layer.name).replace("/", "_")]
			child.set_meta("map_layer_name", layer.name)
			child.set_meta("map_layer_kind", layer.kind)
			child.z_index = index * 128
			# melonJS uses this layer for physics and does not draw it.
			child.visible = layer.visible and layer.name != "Collision"
			map_root.add_child(child)
			child.owner = map_root
			if layer.kind == "objectgroup":
				var object_index := 0
				for obj: Dictionary in layer.objects:
					var node := Node2D.new()
					node.name = "%03d_%s" % [object_index, obj.name if obj.name != "" else "object"]
					node.z_index = 127 - object_index
					node.set_script(OBJECT_SCRIPT)
					node.position = Vector2(obj.x, obj.y)
					node.set("object_type", obj.name)
					node.set("gid", int(obj.gid))
					node.set("size", Vector2i(obj.width, obj.height))
					var properties: Dictionary = obj.properties
					node.set("placement", properties.get("placement", "floor"))
					node.set("behavior", properties.get("behavior", ""))
					node.set("delay", float(properties.get("delay", "0")))
					node.set("fire_duration_min", int(properties.get("fireDurationMin", "20")))
					node.set("fire_duration_max", int(properties.get("fireDurationMax", "160")))
					if int(obj.gid) > 0:
						var info := find_set(sets, int(obj.gid))
						var path := "res://assets/images/%s.png" % info.name
						if ResourceLoader.exists(path):
							var texture: Texture2D = load(path)
							var cols: int = int(info.image_width) / int(info.width)
							var frame: int = int(obj.gid) - int(info.firstgid)
							node.set("preview_texture", texture)
							node.set("preview_region", Rect2((frame % cols) * int(info.width), (frame / cols) * int(info.height), info.width, info.height))
					child.add_child(node)
					node.owner = map_root
					object_index += 1
			index += 1
		var scene := PackedScene.new()
		if scene.pack(map_root) != OK or ResourceSaver.save(scene, "res://scenes/maps/%s.tscn" % entry.id) != OK:
			push_error("Failed to save map " + str(entry.id))
			quit(1)
			return
		map_root.free()
	print("Converted %d maps" % maps.size())
	quit(0)
