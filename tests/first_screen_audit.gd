extends SceneTree

func _initialize():
	call_deferred("run_checks")

func frames(count: int):
	for i in range(count):
		await physics_frame

func check(condition: bool, message: String):
	if not condition:
		push_error(message)
		quit(1)
		return false
	return true

func run_checks():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	var start: Vector2 = player.position
	Input.action_press("ui_right")
	await frames(30)
	Input.action_release("ui_right")
	if not check(player.position.x - start.x >= 40, "Walking failed"): return
	Input.action_press("ui_down")
	await frames(3)
	if not check(player.IsDucking, "Ducking failed"): return
	Input.action_release("ui_down")
	await frames(3)
	var ground_y: float = player.position.y
	Input.action_press("ui_up")
	await frames(12)
	Input.action_release("ui_up")
	if not check(player.position.y < ground_y - 15, "Jump failed"): return
	await frames(70)
	if not check(player.is_on_floor(), "Landing failed"): return
	Input.action_press("fire")
	await frames(2)
	Input.action_release("fire")
	await frames(45)
	if not check(world.TestAmmo() == 98, "Blaster ammo failed"): return
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(100)
	print("Turret after grenade: alive=", world.TestObstacleAlive("turret"), " points=", world.TestPoints())
	if not check(not world.TestObstacleAlive("turret") and world.TestPoints() == 150, "Grenade did not destroy turret for 150 points"): return
	player.position = Vector2(270, ground_y)
	player.velocity = Vector2.ZERO
	await frames(5)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(100)
	print("Cocoon after grenade: alive=", world.TestObstacleAlive("cocoon"), " points=", world.TestPoints())
	if not check(not world.TestObstacleAlive("cocoon") and world.TestPoints() == 300, "Grenade did not destroy cocoon for 150 points"): return
	player.position = Vector2(535, ground_y)
	await frames(10)
	if not check(world.TestZone() == 1, "Transition to zone 2 failed"): return
	print("PASS: movement, duck, jump, blaster, grenades, obstacles, points and transition")
	world.queue_free()
	await process_frame
	quit(0)
