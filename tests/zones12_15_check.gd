extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S12.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await frames(5)
	if world.TestObstacleCount("radar") != 1 or world.TestObstacleCount("missile_guidance") != 1 or not world.TestHasInterceptorCreator() or world.TestMissilePosition().x < 0 or world.TestMapObjectCount("ammo_pack") != 1 or world.TestMapObjectCount("grenade_pack") != 1:
		fail("Zone 12 objects missing")
		return
	for zone in [13, 14, 15]:
		player.position.x = 535
		for i in range(12):
			await physics_frame
			if world.TestZone() == zone - 1: break
		if world.TestZone() != zone - 1:
			fail("Could not load zone " + str(zone))
			return
		await frames(2)
		if zone == 13 and (world.TestObstacleCount("turret") != 1 or world.TestTeleportCount() != 2 or world.TestMapObjectCount("ammo_pack") != 1 or world.TestMapObjectCount("grenade_pack") != 1):
			fail("Zone 13 objects missing")
			return
		if zone == 14 and (world.TestPistonCount() != 1 or world.TestObstacleCount("incubator") != 1 or world.TestEggCount() != 8 or world.TestLauncherCount() != 1 or not world.TestHasInterceptorCreator()):
			fail("Zone 14 objects missing")
			return
		if zone == 15 and (world.TestPistonCount() != 3 or world.TestLauncherCount() != 1 or world.TestTeleportCount() != 2):
			fail("Zone 15 objects missing")
			return
		if DisplayServer.get_name() != "headless":
			await RenderingServer.frame_post_draw
			root.get_texture().get_image().save_png("user://zone%d_check.png" % zone)
	player.position.x = 535
	for i in range(12):
		await physics_frame
		if world.TestZone() == 15: break
	if world.TestZone() != 15:
		fail("Could not load zone 16")
		return
	print("PASS: zones 12-15 load objects and transition to zone 16")
	world.queue_free()
	await process_frame
	quit(0)
