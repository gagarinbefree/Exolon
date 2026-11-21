extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func fail(message: String) -> void:
	push_error(message)
	quit(1)

func run_check() -> void:
	var checked := 0
	for level in range(1, 4):
		for screen in range(1, 26):
			var id := "L%02dS%02d" % [level, screen]
			var scene: PackedScene = load("res://scenes/maps/%s.tscn" % id)
			if scene == null: return fail("Missing Godot map: " + id)
			var map = scene.instantiate()
			var sets: Array = map.get("tilesets")
			var layers := {}
			for node in map.get_children():
				if node is TileMapLayer: layers[str(node.get_meta("map_layer_name"))] = node
			var parser := XMLParser.new()
			if parser.open("res://assets/maps/%s.tmx" % id) != OK: return fail("Missing original map " + id)
			var layer_name := ""
			var width := 0
			var reading_data := false
			while parser.read() == OK:
				if parser.get_node_type() == XMLParser.NODE_ELEMENT:
					if parser.get_node_name() == "layer":
						layer_name = parser.get_named_attribute_value_safe("name")
						width = int(parser.get_named_attribute_value_safe("width"))
					elif parser.get_node_name() == "data" and layer_name != "":
						reading_data = true
				elif parser.get_node_type() == XMLParser.NODE_TEXT and reading_data:
					var bytes := Marshalls.base64_to_raw(parser.get_node_data().strip_edges())
					var tile_layer: TileMapLayer = layers.get(layer_name)
					if tile_layer == null: return fail("Missing layer %s in %s" % [layer_name, id])
					var count := 0
					for i in range(bytes.size() / 4):
						var expected: int = bytes.decode_u32(i * 4)
						if expected == 0: continue
						var cell := Vector2i(i % width, i / width)
						var source := tile_layer.get_cell_source_id(cell)
						var atlas := tile_layer.get_cell_atlas_coords(cell)
						var columns := 0
						for info: Dictionary in sets:
							if int(info.firstgid) == source: columns = int(info.image_width) / int(info.width)
						if columns == 0 or source + atlas.y * columns + atlas.x != expected:
							return fail("Tile differs in %s / %s at %s: expected %d" % [id, layer_name, cell, expected])
						count += 1
					if tile_layer.get_used_cells().size() != count: return fail("Extra tiles in %s / %s" % [id, layer_name])
					checked += count
					reading_data = false
				elif parser.get_node_type() == XMLParser.NODE_ELEMENT_END and parser.get_node_name() == "layer":
					layer_name = ""
			map.free()
	print("PASS: %d native map tiles match all 75 original TMX maps" % checked)
	quit(0)
