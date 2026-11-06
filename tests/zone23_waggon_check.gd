extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S23.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for remaining in [1, 0]:
		Input.action_press("ui_right")
		await frames(100)
		Input.action_release("ui_right")
		await frames(3)
		Input.action_press("fire")
		await frames(37)
		Input.action_release("fire")
		await frames(110)
		print("Waggons left=", world.TestObstacleCount("waggon"), " player=", player.position, " points=", world.TestPoints())
		if world.TestObstacleCount("waggon") != remaining:
			push_error("Waggon not destroyed")
			quit(1)
			return
	if world.TestPoints() < 300:
		push_error("Waggon score not awarded")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(180):
		await physics_frame
		if player.position.x >= 350: break
	Input.action_press("ui_up")
	await frames(30)
	Input.action_release("ui_up")
	for i in range(200):
		await physics_frame
		if world.TestZone() == 23: break
	Input.action_release("ui_right")
	print("Zone 23 exit: zone=", world.TestZone(), " player=", player.position, " top=", world.TestObstacleCount("combined_launcher_top"), " launcher=", world.TestLauncherCount())
	if world.TestZone() != 23 or world.TestObstacleCount("combined_launcher_top") != 1 or world.TestLauncherCount() != 1:
		push_error("Could not enter zone 24")
		quit(1)
		return
	print("PASS: both waggons destroyed and zone 24 entered")
	world.queue_free()
	await process_frame
	quit(0)
