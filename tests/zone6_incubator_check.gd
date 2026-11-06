extends SceneTree

func _initialize():
	call_deferred("check_zone")

func frames(n: int):
	for i in range(n): await physics_frame

func check_zone():
	var world = load("res://scenes/zones/L01S06.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	if world.TestEggCount() != 8 or world.TestConfinedEggCount() != 8:
		push_error("Incubator did not create eight confined eggs")
		quit(1)
		return
	var initial: Vector2 = world.TestFirstEggPosition()
	var moved := false
	for i in range(60):
		await physics_frame
		var egg: Vector2 = world.TestFirstEggPosition()
		if egg.distance_to(initial) > 1: moved = true
		if egg.x < 384 or egg.x > 400 or egg.y < 240 or egg.y > 256:
			push_error("Egg escaped the intact incubator: " + str(egg))
			quit(1)
			return
	if not moved:
		push_error("Egg did not move")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone6_check.png")
	Input.action_press("ui_right")
	for i in range(260):
		await physics_frame
		if player.position.x >= 340: break
	Input.action_release("ui_right")
	await frames(3)
	var before: int = world.TestPoints()
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	for i in range(110):
		await physics_frame
		if not world.TestObstacleAlive("incubator"): break
	print("Incubator alive=", world.TestObstacleAlive("incubator"), " confined=", world.TestConfinedEggCount(), " eggs=", world.TestEggCount(), " points delta=", world.TestPoints() - before)
	if world.TestObstacleAlive("incubator") or world.TestConfinedEggCount() != 0 or world.TestPoints() - before < 150:
		push_error("Grenade did not destroy the incubator and release its eggs")
		quit(1)
		return
	# Isolate a blaster hit by placing the player a short distance to the left
	# of a released egg; the shot itself still uses the normal InputMap.
	await frames(3)
	var egg_target: Vector2 = world.TestFirstEggPosition()
	var eggs_before: int = world.TestEggCount()
	before = world.TestPoints()
	player.position = egg_target + Vector2(-40, 4)
	player.velocity = Vector2.ZERO
	Input.action_press("fire")
	await frames(5)
	Input.action_release("fire")
	if world.TestEggCount() >= eggs_before or world.TestPoints() - before < 50:
		push_error("Blaster did not destroy a released egg for 50 points")
		quit(1)
		return
	print("PASS: confined eggs move, grenade releases them and blaster destroys an egg")
	world.queue_free()
	await process_frame
	quit(0)
