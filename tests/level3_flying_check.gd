extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	for zone in ["L03S02", "L03S05", "L03S07"]:
		var world = load("res://scenes/zones/" + zone + ".tscn").instantiate()
		root.add_child(world)
		var player = world.get_node("Player")
		player.GrantInvulnerability(30.0)
		for i in range(100):
			await physics_frame
			if world.TestFlyingEnemyCount() > 0: break
		if world.TestFlyingEnemyCount() == 0:
			push_error(zone + " enemy did not spawn")
			quit(1)
			return
		var first: Vector2 = world.TestFirstFlyingPosition()
		for i in range(5): await physics_frame
		var second: Vector2 = world.TestFirstFlyingPosition()
		var dx := first.x - second.x
		if zone == "L03S02" and (absf(dx - 15) > 0.1 or absf(first.y - second.y) > 0.1): return fail("Flasher straight phase")
		if zone == "L03S05" and (absf(dx - 12.5) > 0.1 or absf(first.y - second.y - 15) > 0.1): return fail("Fir zig-zag phase")
		if zone == "L03S07" and (absf(dx - 7.5) > 0.1 or absf(first.y - second.y) < 0.1): return fail("Fir swing phase")
		if zone != "L03S07":
			for i in range(100):
				await physics_frame
				if world.TestFirstFlyingPhase() > 0: break
			if world.TestFirstFlyingPhase() < 1: return fail(zone + " checkpoint")
		world.queue_free()
		await process_frame
	print("PASS: flasher and fir movement in third-level zones")
	quit(0)

func fail(message: String):
	push_error(message)
	quit(1)
