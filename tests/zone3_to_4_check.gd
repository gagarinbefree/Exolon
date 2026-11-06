extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(count: int):
	for i in range(count): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S03.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	Input.action_press("ui_right")
	for i in range(100):
		await physics_frame
		if player.position.x >= 90: break
	Input.action_release("ui_right")
	await frames(3)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(110)
	if world.TestObstacleAlive("turret"):
		push_error("Could not destroy turret in zone 3")
		quit(1)
		return
	var ammo: int = world.TestAmmo()
	var grenades: int = world.TestGrenades()
	Input.action_press("ui_right")
	for i in range(450):
		await physics_frame
		if world.TestZone() == 3: break
	Input.action_release("ui_right")
	if world.TestZone() != 3 or world.TestAmmo() != ammo or world.TestGrenades() != grenades or world.TestPoints() != 150:
		push_error("Transition or state carry-over failed")
		quit(1)
		return
	print("PASS: walked from zone 3 into zone 4 with ammo, grenades and points preserved")
	world.queue_free()
	await process_frame
	quit(0)
