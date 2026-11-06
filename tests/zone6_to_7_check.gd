extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L01S06.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	Input.action_press("ui_right")
	for i in range(260):
		await physics_frame
		if player.position.x >= 340: break
	Input.action_release("ui_right")
	for i in range(3): await physics_frame
	Input.action_press("fire")
	for i in range(37): await physics_frame
	Input.action_release("fire")
	for i in range(110):
		await physics_frame
		if not world.TestObstacleAlive("incubator"): break
	if world.TestObstacleAlive("incubator"):
		push_error("Incubator not destroyed")
		quit(1)
		return
	var ammo: int = world.TestAmmo()
	var grenades: int = world.TestGrenades()
	var points: int = world.TestPoints()
	Input.action_press("ui_right")
	for i in range(240):
		await physics_frame
		if world.TestZone() == 6: break
	Input.action_release("ui_right")
	if world.TestZone() != 6 or world.TestAmmo() != ammo or world.TestGrenades() != grenades or world.TestPoints() < points:
		push_error("Transition to zone 7 or state carry-over failed")
		quit(1)
		return
	print("PASS: walked from zone 6 to zone 7 with state preserved")
	world.queue_free()
	await process_frame
	quit(0)
