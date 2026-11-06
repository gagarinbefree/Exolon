extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	for zone in [9, 10, 13, 17, 19]:
		var id = "L03S%02d" % zone
		var world = load("res://scenes/zones/" + id + ".tscn").instantiate()
		root.add_child(world)
		var player = world.get_node("Player")
		player.GrantInvulnerability(30.0)
		var first := Vector2(-100, -100)
		for i in range(120):
			await physics_frame
			first = position_for(world, zone)
			if first.x > 400: break
		if first.x < 400: return fail(id + " enemy did not spawn")
		for i in range(5): await physics_frame
		var second := position_for(world, zone)
		var dx := first.x - second.x
		var dy := first.y - second.y
		if zone in [9, 17] and (absf(dx - 12.5) > 0.1 or absf(dy - 15) > 0.1): return fail(id + " zig-zag")
		if zone == 10 and (absf(dx - 7.5) > 0.1 or absf(dy) > 0.1): return fail(id + " acceleration")
		if zone == 13 and (absf(dx - 15) > 0.1 or absf(dy) > 0.1): return fail(id + " up-and-down")
		if zone == 19 and (absf(dx - 8.5) > 0.1 or absf(dy) < 0.1): return fail(id + " swing")
		world.queue_free()
		await process_frame
	print("PASS: third-level zig-zag, acceleration, up-and-down and swing variants")
	quit(0)

func position_for(world, zone: int) -> Vector2:
	match zone:
		9: return world.TestFirstBubblePosition()
		10: return world.TestFirstFlyingPosition()
		13: return world.TestFirstJellyfishPosition()
		17: return world.TestFirstInterceptorPosition()
		19: return world.TestFirstLousePosition()
	return Vector2(-100, -100)

func fail(message: String):
	push_error(message)
	quit(1)
