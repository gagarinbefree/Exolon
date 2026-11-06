extends SceneTree

func _initialize():
	call_deferred("check_previews")

func check_previews():
	for level in range(1, 4):
		for screen in range(1, 26):
			var name := "L%02dS%02d" % [level, screen]
			var packed = load("res://scenes/zones/" + name + ".tscn")
			if not packed:
				push_error("Missing zone scene: " + name)
				quit(1)
				return
			var zone = packed.instantiate()
			zone.process_mode = Node.PROCESS_MODE_DISABLED
			root.add_child(zone)
			if not zone.has_node("Player") or zone.TestZone() != (level - 1) * 25 + screen - 1:
				push_error("Zone failed to load: " + name)
				quit(1)
				return
			zone.queue_free()
			await process_frame
	print("PASS: all 75 zone scenes load their TMX maps")
	quit(0)
