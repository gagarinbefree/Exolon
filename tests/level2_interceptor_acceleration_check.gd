extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	for zone in ["L02S18", "L02S19", "L02S24"]:
		var world = load("res://scenes/zones/" + zone + ".tscn").instantiate()
		root.add_child(world)
		var player = world.get_node("Player")
		player.GrantInvulnerability(30.0)
		for i in range(70):
			await physics_frame
			if world.TestInterceptorCount() > 0: break
		if world.TestInterceptorCount() == 0:
			push_error(zone + " interceptor did not spawn at configured delay")
			quit(1)
			return
		var first: Vector2 = world.TestFirstInterceptorPosition()
		for i in range(5): await physics_frame
		var second: Vector2 = world.TestFirstInterceptorPosition()
		if absf(first.x - second.x - 7.5) > 0.1 or absf(first.y - second.y) > 0.1:
			push_error(zone + " interceptor normal movement differs from melonJS")
			quit(1)
			return
		for i in range(180):
			await physics_frame
			if world.TestFirstInterceptorFast(): break
		if not world.TestFirstInterceptorFast():
			push_error(zone + " interceptor did not accelerate")
			quit(1)
			return
		world.queue_free()
		await process_frame
	print("PASS: configured interceptor delays and acceleration in level 2")
	quit(0)
