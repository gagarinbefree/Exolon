extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L03S01.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(120.0)
	for zone in range(1, 26):
		if world.TestZone() != zone + 49: return fail("Wrong zone counter at " + str(zone))
		if zone < 25:
			var expected := "L03S%02d" % (zone + 1)
			if world.TestNextLevel() != expected: return fail("Wrong map link at " + str(zone))
			player.position.x = 535
			for i in range(5): await physics_frame
			if world.TestZone() != zone + 50: return fail("Could not transition from " + str(zone))
	if world.TestNextLevel() != "L01S01": return fail("Wrong final map link")
	print("PASS: all third-level maps and boundary transitions through zone 25")
	world.queue_free()
	await process_frame
	quit(0)

func fail(message: String):
	push_error(message)
	quit(1)
