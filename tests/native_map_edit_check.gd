extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func fail(message: String) -> void:
	push_error(message)
	quit(1)

func run_check() -> void:
	# Override the cached map only in this test process. The project file is never written.
	var path := "res://scenes/maps/L01S01.tscn"
	var source: PackedScene = load(path)
	var map := source.instantiate()
	var spawn: Node2D
	var turret: Node2D
	var collision: TileMapLayer
	for layer in map.get_children():
		if layer is TileMapLayer and layer.get_meta("map_layer_name") == "Collision": collision = layer
		for node in layer.get_children():
			if node.get("object_type") == "vitorc": spawn = node
			if node.get("object_type") == "turret": turret = node
	if spawn == null or turret == null or collision == null: return fail("Expected editable map nodes")
	spawn.position.x = 48
	turret.position.x += 16
	var turret_x: int = int(turret.position.x)
	var turret_y: int = int(turret.position.y)
	var existing_cell: Vector2i = collision.get_used_cells()[0]
	var source_id := collision.get_cell_source_id(existing_cell)
	var atlas := collision.get_cell_atlas_coords(existing_cell)
	var extra := Vector2i(15, 8)
	collision.set_cell(extra, source_id, atlas)
	map.set("next_level", "L01S03")
	var changed := PackedScene.new()
	if changed.pack(map) != OK: return fail("Could not pack edited map")
	changed.take_over_path(path)
	map.free()
	var world = load("res://scenes/zones/L01S01.tscn").instantiate()
	root.add_child(world)
	await physics_frame
	var player = world.get_node("Player")
	if absf(player.position.x - 72) > 1 or world.TestObjectZ("turret", turret_x, turret_y) < 0:
		return fail("The game ignored edited object positions")
	if world.TestNextLevel() != "L01S03": return fail("The game ignored the edited next zone")
	var found := false
	for node in world.get_children():
		if node is StaticBody2D and node.position == Vector2(extra * 16 + Vector2i(8, 8)): found = true
	if not found: return fail("The game ignored an edited collision tile")
	world.queue_free()
	await process_frame
	print("PASS: edited spawn, object, collision tile and next zone affect the game")
	quit(0)
