extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S09.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await frames(5)
	var first: Vector2 = world.TestMissilePosition()
	await frames(10)
	var next: Vector2 = world.TestMissilePosition()
	if absf(first.x - next.x - 15.0) > 0.1 or next.y >= first.y:
		fail("Missile normal speed or vertical tracking failed: %s -> %s" % [first, next])
		return
	for i in range(200):
		await physics_frame
		if world.TestMissileFast(): break
	if not world.TestMissileFast():
		fail("Missile did not accelerate")
		return
	first = world.TestMissilePosition()
	await frames(5)
	next = world.TestMissilePosition()
	if absf(first.x - next.x - 15.0) > 0.1:
		fail("Missile fast speed failed: %s -> %s" % [first, next])
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone9_check.png")
	# Position near the guidance unit to isolate a grenade hit; firing uses input.
	player.position = Vector2(270, 272)
	player.velocity = Vector2.ZERO
	await frames(3)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	for i in range(120):
		await physics_frame
		if not world.TestObstacleAlive("missile_guidance"): break
	if world.TestObstacleAlive("missile_guidance") or world.TestMissilePosition().x >= 0 or world.TestAwardRemaining() <= 0:
		fail("Guidance destruction did not remove missile and start bonus")
		return
	var spawned: int = world.TestMissileSpawnCount()
	await frames(130)
	if world.TestPoints() != 1000 or world.TestAwardRemaining() != 0 or world.TestMissileSpawnCount() != spawned:
		fail("Guidance bonus or missile shutdown failed: " + str(world.TestPoints()))
		return
	print("PASS: tracking, acceleration, guidance destruction and 1000-point bonus")
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S09.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	for i in range(450):
		await physics_frame
		if player.IsDead: break
	if not player.IsDead:
		fail("Missile did not kill vulnerable player")
		return
	print("PASS: missile kills vulnerable player")
	world.queue_free()
	await process_frame
	quit(0)
