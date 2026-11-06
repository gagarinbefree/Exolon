extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	for zone in ["L02S13", "L02S14"]:
		var world = load("res://scenes/zones/" + zone + ".tscn").instantiate()
		root.add_child(world)
		var player = world.get_node("Player")
		player.GrantInvulnerability(30.0)
		for i in range(120):
			await physics_frame
			if world.TestJellyfishCount() > 0: break
		var first: Vector2 = world.TestFirstJellyfishPosition()
		for i in range(5): await physics_frame
		var second: Vector2 = world.TestFirstJellyfishPosition()
		if absf(first.x - second.x - 7.5) > 0.1 or absf(first.y - second.y) < 0.1:
			push_error(zone + " jellyfish did not fly with the expected swing")
			quit(1)
			return
		for i in range(180):
			await physics_frame
			if world.TestFirstJellyfishFast(): break
		if not world.TestFirstJellyfishFast():
			push_error(zone + " jellyfish did not accelerate")
			quit(1)
			return
		world.queue_free()
		await process_frame
	print("PASS: L02S13 and L02S14 jellyfish swing and acceleration")
	quit(0)
