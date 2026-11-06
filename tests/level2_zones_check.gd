extends SceneTree

func _initialize():
	call_deferred("run_check")

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L02S01.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	player.ToggleOutfit()
	if world.TestObstacleCount("fungus") != 2 or world.TestObstacleCount("cocoon") != 1:
		fail("Level 2 zone 1 objects missing")
		return
	for zone in range(2, 11):
		player.position.x = 535
		for i in range(12):
			await physics_frame
			if world.TestZone() == 24 + zone: break
		if world.TestZone() != 24 + zone or not player.IsArmored:
			fail("Transition or outfit preservation failed at zone " + str(zone))
			return
		if zone == 2 and (world.TestObstacleCount("waggon") != 2 or world.TestMineCount() != 1 or not world.TestHasLouseCreator()):
			fail("Level 2 zone 2 objects missing")
			return
		if zone == 4 and (world.TestObstacleCount("incubator") != 1 or world.TestEggCount() != 8 or world.TestLauncherCount() != 1):
			fail("Level 2 zone 4 objects missing")
			return
		if zone == 6 and (world.TestPistonCount() != 1 or world.TestEggCount() != 12 or world.TestObstacleCount("turret") != 1):
			fail("Level 2 zone 6 objects missing")
			return
		if zone == 7 and world.TestPistonCount() != 3:
			fail("Level 2 zone 7 pistons missing")
			return
		if zone == 8 and (world.TestMineCount() != 2 or world.TestTeleportCount() != 2 or world.TestObstacleCount("combined_launcher_top") != 1 or world.TestLauncherCount() != 1):
			fail("Level 2 zone 8 objects missing")
			return
		if zone == 10 and (world.TestMineCount() != 3 or world.TestEggCount() != 12 or world.TestCapsuleCount() != 1):
			fail("Level 2 zone 10 objects missing")
			return
		if DisplayServer.get_name() != "headless" and zone in [2, 4, 8, 10]:
			await RenderingServer.frame_post_draw
			root.get_texture().get_image().save_png("user://level2_zone%d_check.png" % zone)
	print("PASS: level 2 zones 1-10 load objects and preserve outfit across transitions")
	world.queue_free()
	await process_frame
	quit(0)
