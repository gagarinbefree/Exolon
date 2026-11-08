extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S03.tscn").instantiate()
	root.add_child(world)
	await frames(5)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	# This test starts with the third zone; interactions use normal input.
	if world.TestZone() != 2 or world.TestNextLevel() != "L01S04":
		fail("Third zone failed to load")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone3_check.png")
	# Spend one bullet so pickup has an observable effect.
	Input.action_press("fire")
	await frames(2)
	Input.action_release("fire")
	await frames(5)
	if world.TestAmmo() != 98:
		fail("Could not spend ammo")
		return
	Input.action_press("ui_right")
	for i in range(45):
		await physics_frame
		if player.position.x >= 80: break
	Input.action_release("ui_right")
	await frames(3)
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	print("Teleport up: ", player.position, " flashes=", world.TestTeleportFlashCount())
	if player.position.x < 350 or player.position.y > 155 or world.TestTeleportFlashCount() != 2:
		fail("Teleport up or its two flashes failed")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone3_teleport_check.png")
	await frames(5)
	Input.action_press("ui_right")
	for i in range(70):
		await physics_frame
		if world.TestAmmoPackCollected(): break
	Input.action_release("ui_right")
	if not world.TestAmmoPackCollected() or world.TestAmmo() != 99:
		fail("Ammo pickup failed")
		return
	Input.action_press("ui_left")
	for i in range(60):
		await physics_frame
		if player.position.x <= 376: break
	Input.action_release("ui_left")
	await frames(3)
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	print("Teleport down: ", player.position)
	if player.position.x > 120 or player.position.y < 245:
		fail("Teleport back failed")
		return
	# Face right and destroy the turret from beside the lower teleport.
	Input.action_press("ui_right")
	await frames(2)
	Input.action_release("ui_right")
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(110)
	if world.TestObstacleAlive("turret") or world.TestPoints() != 150:
		fail("Third-zone turret destruction failed")
		return
	player.GrantInvulnerability(0.0)
	await frames(3)
	Input.action_press("ui_right")
	for i in range(240):
		await physics_frame
		if player.IsDead or player.position.x >= 420: break
	Input.action_release("ui_right")
	# The piston starts after a random delay. Wait on it for a full cycle
	# instead of walking past it and accidentally entering the next zone.
	if not player.IsDead:
		for i in range(420):
			await physics_frame
			if player.IsDead: break
	print("Piston: player=", player.position, " rise=", world.TestPistonRise(), " dead=", player.IsDead)
	if not player.IsDead or player.position.x < 405:
		for i in range(player.get_slide_collision_count()):
			print("Collision: ", player.get_slide_collision(i).get_collider(), " normal=", player.get_slide_collision(i).get_normal())
		fail("Piston did not kill the player")
		return
	print("PASS: teleports, flashes, ammo, turret and lethal piston")
	world.queue_free()
	await process_frame
	quit(0)
