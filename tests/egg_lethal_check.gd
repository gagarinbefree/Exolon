extends SceneTree

func _initialize():
	root.title = "Exolon - egg collision check"
	call_deferred("check_collision")

func frames(n: int):
	for i in range(n): await physics_frame

func check_collision():
	var world = load("res://scenes/zones/L01S06.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	# Keep the approach reproducible; immunity is explicitly removed before contact.
	player.GrantInvulnerability(30.0)
	Input.action_press("ui_right")
	for i in range(260):
		await physics_frame
		if player.position.x >= 340: break
	Input.action_release("ui_right")
	await frames(3)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	for i in range(110):
		await physics_frame
		if not world.TestObstacleAlive("incubator"): break
	if world.TestObstacleAlive("incubator") or world.TestEggCount() == 0:
		push_error("Could not release the eggs")
		quit(1)
		return
	player.GrantInvulnerability(0.0)
	await frames(3)
	if player.IsInvincible:
		push_error("Player still invincible before egg collision")
		quit(1)
		return
	# Position near one released egg to isolate the collision from the bubble enemy.
	var egg: Vector2 = world.TestFirstEggPosition()
	var count_before: int = world.TestEggCount()
	var points_before: int = world.TestPoints()
	player.position = egg + Vector2(8, 8)
	player.velocity = Vector2.ZERO
	await frames(2)
	print("Egg collision: dead=", player.IsDead, " invincible=", player.IsInvincible, " eggs before=", count_before, " after=", world.TestEggCount(), " points delta=", world.TestPoints() - points_before)
	if not player.IsDead or world.TestEggCount() >= count_before or world.TestPoints() - points_before < 50:
		push_error("Released egg did not kill the vulnerable player")
		quit(1)
		return
	print("PASS: released egg kills a vulnerable player and disappears")
	if DisplayServer.get_name() != "headless": await create_timer(1.0).timeout
	world.queue_free()
	await process_frame
	quit(0)
