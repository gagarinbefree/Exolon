extends SceneTree

func _initialize():
	call_deferred("run_checks")

func run_checks():
	var world = load("res://scenes/zones/L01S07.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	var hit := false
	for i in range(1200):
		await physics_frame
		var bullet: Vector2 = world.TestLauncherBulletPosition()
		if bullet.x >= 120 and bullet.x <= 210 and absf(bullet.y - 272) < 0.1:
			var before: int = world.TestPoints()
			Input.action_press("fire")
			await physics_frame
			Input.action_release("fire")
			for j in range(35):
				await physics_frame
				if world.TestPoints() >= before + 50:
					hit = true
					break
			if hit: break
	if not hit:
		push_error("Blaster did not destroy a launcher bullet")
		quit(1)
		return
	print("PASS: blaster destroyed launcher bullet for 50 points")
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S07.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	for i in range(500):
		await physics_frame
		if player.IsDead: break
	if not player.IsDead:
		push_error("Launcher bullet did not kill vulnerable player")
		quit(1)
		return
	print("PASS: launcher bullet kills a vulnerable player")
	world.queue_free()
	await process_frame
	quit(0)
