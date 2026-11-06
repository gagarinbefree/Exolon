extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S08.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(20.0)
	await frames(3)
	if world.TestMineCount() != 3 or world.TestObstacleCount("turret") != 2:
		push_error("Zone 8 objects missing")
		quit(1)
		return
	var upper := false
	var lower := false
	for i in range(350):
		await physics_frame
		if world.TestTurretBulletsAtY(70.0) > 0: upper = true
		if world.TestTurretBulletsAtY(246.0) > 0: lower = true
		if upper and lower: break
	if not upper or not lower:
		push_error("Both turrets did not fire from the correct positions")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone8_check.png")
	# Reproduce the controlled melonJS collision while invulnerable.
	player.position = Vector2(164, 272)
	player.velocity = Vector2.ZERO
	await frames(2)
	if player.IsDead or world.TestMineCount() != 2 or world.TestMineFireCount() != 1 or world.TestPoints() != 0:
		push_error("Invulnerable mine collision did not match melonJS")
		quit(1)
		return
	var observed_frames := {}
	for i in range(30):
		await physics_frame
		observed_frames[world.TestMineFireFrame(0)] = true
	if observed_frames.size() < 3:
		push_error("Mine fire did not animate")
		quit(1)
		return
	player.GrantInvulnerability(0.0)
	await frames(3)
	player.position = Vector2(244, 272)
	player.velocity = Vector2.ZERO
	await frames(2)
	print("Mine collision: dead=", player.IsDead, " mines=", world.TestMineCount(), " fires=", world.TestMineFireCount(), " points=", world.TestPoints())
	if not player.IsDead or world.TestMineCount() != 1 or world.TestMineFireCount() != 2 or world.TestPoints() != 0:
		push_error("Mine did not kill the vulnerable player")
		quit(1)
		return
	print("PASS: both turrets, mine collisions and animated fire")
	world.queue_free()
	await process_frame
	quit(0)
