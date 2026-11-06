extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var checked := 0
	for level in range(1, 4):
		for screen in range(1, 26):
			var id = "L%02dS%02d" % [level, screen]
			var parser := XMLParser.new()
			if parser.open("res://assets/maps/" + id + ".tmx") != OK:
				push_error("Cannot open " + id)
				quit(1)
				return
			var layer := -1
			var object_index := 0
			var objects := []
			var player_rank := 0
			while parser.read() == OK:
				if parser.get_node_type() != XMLParser.NODE_ELEMENT: continue
				var tag := parser.get_node_name()
				if tag in ["layer", "objectgroup", "imagelayer"]:
					layer += 1
					object_index = 0
				elif tag == "object":
					var item = {"name": parser.get_named_attribute_value_safe("name"), "x": int(parser.get_named_attribute_value_safe("x")), "y": int(parser.get_named_attribute_value_safe("y")), "rank": layer * 128 + 127 - object_index}
					object_index += 1
					objects.append(item)
					if item.name == "vitorc": player_rank = item.rank
			var world = load("res://scenes/zones/" + id + ".tscn").instantiate()
			root.add_child(world)
			var player = world.get_node("Player")
			for item in objects:
				if item.name == "vitorc": continue
				var actual: int = world.TestObjectZ(item.name, item.x, item.y)
				if actual == -1 or signi(actual - player.z_index) != signi(item.rank - player_rank):
					push_error("Wrong object layer in %s: %s (%d, %d), z=%d" % [id, item.name, item.x, item.y, actual])
					quit(1)
					return
				checked += 1
			world.queue_free()
			await process_frame
	print("PASS: %d map objects have the expected order relative to the player across 75 zones" % checked)
	quit(0)
