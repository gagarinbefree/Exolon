extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L02S11.tscn").instantiate()
	root.add_child(world)
	for i in range(3): await physics_frame
	var previous: int = world.TestBeamFrame()
	var changes := 0
	for i in range(24):
		await physics_frame
		var current: int = world.TestBeamFrame()
		if current != previous:
			if current != (previous + 1) % 8:
				push_error("Beam skipped animation frames")
				quit(1)
				return
			changes += 1
		previous = current
	if changes != 8:
		push_error("Beam did not advance every three physics updates: " + str(changes))
		quit(1)
		return
	print("PASS: beam animation advances every three physics updates")
	world.queue_free()
	await process_frame
	quit(0)
