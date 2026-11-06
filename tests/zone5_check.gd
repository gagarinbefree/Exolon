extends SceneTree

func _initialize():
	call_deferred("check_zone")

func check_zone():
	var world = load("res://scenes/zones/L01S05.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	Input.action_press("ui_right")
	for i in range(160): await physics_frame
	Input.action_release("ui_right")
	Input.action_press("fire")
	for i in range(37): await physics_frame
	Input.action_release("fire")
	for i in range(110): await physics_frame
	if world.TestObstacleAlive("radar"):
		push_error("Zone 5 radar not destroyed")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(300):
		await physics_frame
		if world.TestZone() == 5: break
	Input.action_release("ui_right")
	if world.TestZone() != 5 or world.TestNextLevel() != "L01S07" or world.TestEggCount() != 8:
		push_error("Zone 5 exit handling failed: " + str(player.position))
		quit(1)
		return
	print("PASS: zone 5 radar, bubbles and transition to zone 6")
	world.queue_free()
	await process_frame
	quit(0)
