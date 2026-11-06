extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S17.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await frames(3)
	if world.TestMineCount() != 2 or world.TestObstacleCount("cocoon") != 1 or world.TestObstacleCount("turret") != 1 or not world.TestHasInterceptorCreator():
		fail("Zone 17 objects missing")
		return
	for zone in range(18, 23):
		player.position.x = 535
		for i in range(12):
			await physics_frame
			if world.TestZone() == zone - 1: break
		if world.TestZone() != zone - 1:
			fail("Could not load zone " + str(zone))
			return
		await frames(2)
		if zone == 18 and (world.TestPistonCount() != 1 or world.TestTeleportCount() != 2 or not world.TestHasBubbleCreator() or world.TestMapObjectCount("ammo_pack") != 1 or world.TestMapObjectCount("grenade_pack") != 1):
			fail("Zone 18 objects missing")
			return
		if zone == 19 and (world.TestMineCount() != 1 or world.TestObstacleCount("incubator") != 1 or world.TestObstacleCount("rocket") != 1 or world.TestEggCount() != 8 or world.TestTeleportCount() != 2):
			fail("Zone 19 objects missing")
			return
		if zone == 20 and (world.TestObstacleCount("turret") != 1 or not world.TestHasInterceptorCreator()):
			fail("Zone 20 objects missing")
			return
		if zone == 21 and (world.TestPistonCount() != 2 or world.TestEggCount() != 12 or world.TestLauncherCount() != 1):
			fail("Zone 21 objects missing")
			return
		if zone == 22 and (world.TestEggCount() != 12 or world.TestObstacleCount("rocket") != 2 or not world.TestHasBubbleCreator() or world.TestMapObjectCount("ammo_pack") != 1 or world.TestMapObjectCount("grenade_pack") != 1):
			fail("Zone 22 objects missing")
			return
		if DisplayServer.get_name() != "headless":
			await RenderingServer.frame_post_draw
			root.get_texture().get_image().save_png("user://zone%d_check.png" % zone)
	player.position.x = 535
	for i in range(12):
		await physics_frame
		if world.TestZone() == 22: break
	if world.TestZone() != 22 or world.TestObstacleCount("waggon") != 2:
		fail("Transition to zone 23 failed")
		return
	print("PASS: zones 17-22 object setup and transition to zone 23")
	world.queue_free()
	await process_frame
	quit(0)
