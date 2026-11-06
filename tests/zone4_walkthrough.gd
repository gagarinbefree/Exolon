extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S04.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(50.0)
	for remaining in [1, 0]:
		Input.action_press("ui_right")
		await frames(260)
		Input.action_release("ui_right")
		await frames(3)
		print("Before rocket: x=", player.position.x, " y=", player.position.y, " rockets=", world.TestObstacleCount("rocket"))
		Input.action_press("fire")
		await frames(37)
		Input.action_release("fire")
		await frames(100)
		if world.TestObstacleCount("rocket") != remaining:
			push_error("Rocket not destroyed")
			quit(1)
			return
	if not world.TestGrenadePackCollected():
		push_error("Grenade pack not collected")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(230):
		await physics_frame
		if world.TestZone() == 4: break
	Input.action_release("ui_right")
	if world.TestZone() != 4 or world.TestNextLevel() != "L01S06" or world.TestObstacleCount("radar") != 1:
		push_error("Transition to zone 5 failed")
		quit(1)
		return
	print("PASS: zone 4 rockets, pack and transition to zone 5")
	world.queue_free()
	await process_frame
	quit(0)
