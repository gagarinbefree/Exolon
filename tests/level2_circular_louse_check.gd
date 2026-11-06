extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L02S12.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for i in range(120):
		await physics_frame
		if world.TestLouseCount() > 0: break
	var first: Vector2 = world.TestFirstLousePosition()
	for i in range(5): await physics_frame
	var second: Vector2 = world.TestFirstLousePosition()
	if absf(first.x - second.x - 15.0) > 0.1 or absf(first.y - second.y) > 0.1:
		push_error("Circular louse did not fly straight at original speed")
		quit(1)
		return
	for i in range(120):
		await physics_frame
		if world.TestFirstLouseCircular(): break
	if not world.TestFirstLouseCircular():
		push_error("Louse did not enter circular motion")
		quit(1)
		return
	first = world.TestFirstLousePosition()
	for i in range(10): await physics_frame
	second = world.TestFirstLousePosition()
	if second.y >= first.y or second.x >= first.x:
		push_error("Louse circular trajectory has the wrong direction")
		quit(1)
		return
	print("PASS: L02S12 louse straight and circular movement")
	world.queue_free()
	await process_frame
	quit(0)
