extends SceneTree

func _initialize():
	call_deferred("run_walkthrough")

func frames(count: int):
	for i in range(count): await physics_frame

func grenade():
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(105)

func run_walkthrough():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(90.0)
	for target_x in [135.0, 270.0]:
		Input.action_press("ui_right")
		while player.position.x < target_x: await physics_frame
		Input.action_release("ui_right")
		await frames(3)
		await grenade()
	Input.action_press("ui_right")
	for i in range(240):
		await physics_frame
		if world.TestZone() == 1: break
	Input.action_release("ui_right")
	if world.TestZone() != 1 or world.TestNextLevel() != "L01S03":
		push_error("Did not load zone 2 or its TMX nextLevel")
		quit(1)
		return
	print("Zone 2 entered: radar=", world.TestObstacleCount("radar"), " rockets=", world.TestObstacleCount("rocket"), " grenades=", world.TestGrenades())
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone2_check.png")
	for expected in [3, 2, 1, 0]:
		Input.action_press("ui_right")
		await frames(85)
		Input.action_release("ui_right")
		await frames(3)
		await grenade()
		print("Obstacle result: player=", player.position, " radar=", world.TestObstacleCount("radar"), " rockets=", world.TestObstacleCount("rocket"), " points=", world.TestPoints())
		if world.TestObstacleCount("radar") != 0 or world.TestObstacleCount("rocket") != expected:
			push_error("Zone 2 obstacle was not destroyed")
			quit(1)
			return
	var grenades_before: int = world.TestGrenades()
	var collected := false
	Input.action_press("ui_right")
	for i in range(200):
		await physics_frame
		if world.TestGrenadePackCollected(): collected = true
		if world.TestZone() == 2: break
	Input.action_release("ui_right")
	if not collected or grenades_before >= 10 or world.TestGrenades() != 10 or world.TestZone() != 2 or world.TestPoints() != 900:
		push_error("Pickup or transition failed: collected=%s grenades=%d zone=%d points=%d" % [collected, world.TestGrenades(), world.TestZone(), world.TestPoints()])
		quit(1)
		return
	print("PASS: walked zones 1 and 2, destroyed obstacles, collected grenades and entered zone 3")
	world.queue_free()
	await process_frame
	quit(0)
