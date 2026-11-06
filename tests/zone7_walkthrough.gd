extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func move_right_to(player, x: float, limit: int):
	Input.action_press("ui_right")
	for i in range(limit):
		await physics_frame
		if player.position.x >= x: break
	Input.action_release("ui_right")

func run_check():
	var world = load("res://scenes/zones/L01S07.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(40.0)
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone7_check.png")
	await move_right_to(player, 330, 240)
	if not world.TestLauncherCaptured():
		push_error("Launcher was not captured")
		quit(1)
		return
	Input.action_press("ui_right")
	Input.action_press("ui_up")
	await frames(30)
	Input.action_release("ui_up")
	await frames(35)
	Input.action_release("ui_right")
	await frames(10)
	print("After platform jump: ", player.position)
	if player.position.x < 390 or not player.is_on_floor():
		push_error("Could not jump to lower teleport")
		quit(1)
		return
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	await frames(5)
	print("Upper teleport: ", player.position)
	if player.position.x < 140 or player.position.x > 200 or player.position.y > 110:
		push_error("Could not teleport to upper platform")
		quit(1)
		return
	await move_right_to(player, 225, 60)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(110)
	if world.TestObstacleAlive("rocket"):
		push_error("Upper rocket survived")
		quit(1)
		return
	await move_right_to(player, 485, 220)
	await frames(245)
	if not world.TestGrenadePackCollected() or not world.TestAmmoPackCollected() or world.TestPoints() != 2150 or world.TestZone() != 6:
		push_error("Zone 7 pickup, score or boundary failed: " + str(player.position) + " points=" + str(world.TestPoints()))
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(90):
		await physics_frame
		if world.TestZone() == 7: break
	Input.action_release("ui_right")
	if world.TestZone() != 7 or world.TestMineCount() != 3 or world.TestPoints() != 2150:
		push_error("Transition from zone 7 to zone 8 failed")
		quit(1)
		return
	print("PASS: capture, jump, teleport, upper rocket, supplies and transition to zone 8")
	world.queue_free()
	await process_frame
	quit(0)
