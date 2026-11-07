extends SceneTree

func _initialize() -> void:
	call_deferred("run_checks")

func run_checks() -> void:
	var world = load("res://scenes/zones/L01S01.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	Input.action_press("ui_down")
	var crossing := false
	for i in range(1200):
		await physics_frame
		var bullet: Vector2 = world.TestTurretBulletPosition()
		if player.IsDucking and bullet.x < player.position.x and bullet.x > player.position.x - 20:
			crossing = true
			break
		if player.IsDead:
			push_error("Turret killed the crouching player")
			quit(1)
			return
	if not crossing or player.IsDead:
		push_error("Turret bullet did not pass the crouching player")
		quit(1)
		return
	print("PASS: turret shot passed over crouching player")
	Input.action_release("ui_down")
	for i in range(1200):
		await physics_frame
		if player.IsDead: break
	if not player.IsDead:
		push_error("Turret shot did not kill standing player")
		quit(1)
		return
	print("PASS: turret shot killed standing player")
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S07.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	Input.action_press("ui_down")
	for i in range(1200):
		await physics_frame
		if player.IsDead: break
	Input.action_release("ui_down")
	if not player.IsDead:
		push_error("Low launcher shot did not kill crouching player")
		quit(1)
		return
	print("PASS: low launcher shot killed crouching player")
	world.queue_free()
	await process_frame
	quit(0)
