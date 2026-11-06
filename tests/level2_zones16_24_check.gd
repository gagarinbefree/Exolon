extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L02S16.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	for zone in range(16, 25):
		if world.TestZone() != zone + 24:
			push_error("Wrong level 2 zone: " + str(zone))
			quit(1)
			return
		match zone:
			16:
				if world.TestObstacleCount("incubator") != 2 or world.TestObstacleCount("rocket") != 2: return fail("Zone 16")
			17:
				if world.TestObstacleCount("beam") != 1 or world.TestTeleportCount() != 2: return fail("Zone 17")
			18, 19, 24:
				if not world.TestHasInterceptorCreator(): return fail("Zone " + str(zone))
			20:
				if not world.TestHasBubbleCreator() or world.TestObstacleCount("rocket") != 2: return fail("Zone 20")
			21:
				if world.TestTeleportCount() != 2 or world.TestLauncherCount() != 1: return fail("Zone 21")
			22:
				if world.TestLauncherCount() != 2 or world.TestObstacleCount("combined_launcher_top") != 2: return fail("Zone 22")
			23:
				if world.TestPistonCount() != 3: return fail("Zone 23")
		if zone < 24:
			player.position.x = 535
			for i in range(5): await physics_frame
	print("PASS: level 2 zones 16-24 load expected objects and transitions")
	world.queue_free()
	await process_frame
	quit(0)

func fail(message: String):
	push_error(message + " objects not loaded")
	quit(1)
