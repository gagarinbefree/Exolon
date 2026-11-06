extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L01S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.position = Vector2(304, 256)
	for i in range(5): await physics_frame
	if not world.TestLevelCompleteVisible():
		push_error("Completion window did not open")
		quit(1)
		return
	var initial: int = world.TestLightFrame()
	for i in range(3): await physics_frame
	if world.TestLightFrame() != (initial + 1) % 8:
		push_error("Ambient lights stopped during level completion")
		quit(1)
		return
	print("PASS: ambient lights keep animating during level completion")
	world.queue_free()
	await process_frame
	quit(0)
