extends SceneTree

func _initialize():
	call_deferred("run_walkthrough")

func frames(count: int):
	for i in range(count): await physics_frame

func run_walkthrough():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	for target_x in [135.0, 270.0]:
		Input.action_press("ui_right")
		for i in range(180):
			await physics_frame
			if player.position.x >= target_x: break
		Input.action_release("ui_right")
		await frames(3)
		if player.position.x < target_x - 2:
			push_error("Walkthrough blocked before firing position: " + str(player.position))
			quit(1)
			return
		Input.action_press("fire")
		await frames(37)
		Input.action_release("fire")
		await frames(110)
		if (target_x == 135.0 and world.TestObstacleAlive("turret")) or (target_x == 270.0 and world.TestObstacleAlive("cocoon")):
			push_error("Obstacle survived grenade at " + str(target_x))
			quit(1)
			return
	Input.action_press("ui_right")
	for i in range(240):
		await physics_frame
		if world.TestZone() == 1: break
	Input.action_release("ui_right")
	if world.TestZone() != 1 or world.TestPoints() != 300:
		push_error("Walkthrough did not reach zone 2 with 300 points: " + str(player.position))
		quit(1)
		return
	print("PASS: walked from spawn, destroyed both obstacles and entered zone 2; points=", world.TestPoints())
	world.queue_free()
	await process_frame
	quit(0)
