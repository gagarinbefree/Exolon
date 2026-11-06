extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S08.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	Input.action_press("ui_right")
	for i in range(400):
		await physics_frame
		if player.position.x >= 360: break
	Input.action_release("ui_right")
	await frames(3)
	print("Approach: position=", player.position, " mines=", world.TestMineCount(), " fire=", world.TestMineFireCount())
	if player.position.x < 350 or world.TestMineCount() != 0 or world.TestMineFireCount() != 3:
		push_error("Could not walk across all three mines")
		quit(1)
		return
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(110)
	if world.TestObstacleCount("turret") != 1 or world.TestPoints() != 150:
		push_error("Lower turret not destroyed for 150 points")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(180):
		await physics_frame
		if player.position.x >= 485: break
	Input.action_release("ui_right")
	if player.position.x < 485 or world.TestZone() != 7 or world.TestNextLevel() != "L01S09":
		push_error("Did not reach zone 8 boundary")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(90):
		await physics_frame
		if world.TestZone() == 8: break
	Input.action_release("ui_right")
	if world.TestZone() != 8 or not world.TestObstacleAlive("missile_guidance") or world.TestPoints() != 150:
		push_error("Transition from zone 8 to zone 9 failed")
		quit(1)
		return
	print("PASS: crossed three mines, destroyed lower turret and reached zone 9")
	world.queue_free()
	await process_frame
	quit(0)
