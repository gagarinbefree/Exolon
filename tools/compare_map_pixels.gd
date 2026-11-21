extends SceneTree

const REFERENCE = preload("res://tools/map_reference_draw.gd")

func _initialize() -> void:
	call_deferred("run")

func run() -> void:
	var entries: Array = JSON.parse_string(FileAccess.get_file_as_string("res://artifacts/map_conversion.json"))
	var failures := 0
	for entry in entries:
		var left := SubViewport.new()
		var right := SubViewport.new()
		for viewport in [left, right]:
			viewport.size = Vector2i(512, 352)
			viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
			root.add_child(viewport)
		var map = load("res://scenes/maps/%s.tscn" % entry.id).instantiate()
		map.set_meta("preview_for_test", true)
		for layer in map.get_children():
			if layer.get_meta("map_layer_kind", "") == "objectgroup":
				for object in layer.get_children():
					object.set_meta("preview_for_test", true)
		left.add_child(map)
		var reference = REFERENCE.new()
		reference.setup(entry)
		right.add_child(reference)
		await process_frame
		await RenderingServer.frame_post_draw
		var actual := left.get_texture().get_image()
		var expected := right.get_texture().get_image()
		var a := actual.get_data()
		var b := expected.get_data()
		var different := 0
		for i in range(0, a.size(), 4):
			if a[i] != b[i] or a[i + 1] != b[i + 1] or a[i + 2] != b[i + 2]:
				different += 1
		if different:
			failures += 1
			print("%s: %d differing pixels" % [entry.id, different])
			if failures <= 3:
				actual.save_png("res://artifacts/%s-native-map.png" % entry.id)
				expected.save_png("res://artifacts/%s-reference-map.png" % entry.id)
		left.queue_free()
		right.queue_free()
	print("Map comparisons: %d/%d identical" % [entries.size() - failures, entries.size()])
	quit(1 if failures else 0)
