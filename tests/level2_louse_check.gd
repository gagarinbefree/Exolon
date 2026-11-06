extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func wait_target(world, x: float):
	for i in range(300):
		await physics_frame
		if world.TestLouseCount() > 0 and world.TestFirstLousePosition().x < x: return

func run_check():
	var world = load("res://scenes/zones/L02S02.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for i in range(100):
		await physics_frame
		if world.TestLouseCount() > 0: break
	var first: Vector2 = world.TestFirstLousePosition()
	await frames(5)
	var second: Vector2 = world.TestFirstLousePosition()
	if first.x < 500 or absf(first.x - second.x - 7.5) > 0.1 or absf(first.y - second.y) > 0.1:
		fail("Louse normal movement differs from melonJS")
		return
	for i in range(180):
		await physics_frame
		if world.TestFirstLouseFast(): break
	if not world.TestFirstLouseFast():
		fail("Louse did not accelerate")
		return
	first = world.TestFirstLousePosition()
	await frames(5)
	second = world.TestFirstLousePosition()
	if absf(first.x - second.x - 20) > 0.1:
		fail("Louse fast speed differs from melonJS")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://level2_zone2_check.png")
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L02S02.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await wait_target(world, 430)
	var target: Vector2 = world.TestFirstLousePosition()
	var before: int = world.TestPoints()
	player.position = target + Vector2(-40, 4)
	player.velocity = Vector2.ZERO
	Input.action_press("fire")
	await frames(6)
	Input.action_release("fire")
	if world.TestPoints() < before + 150:
		fail("Blaster did not destroy louse for 150 points")
		return
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L02S02.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	await wait_target(world, 390)
	target = world.TestFirstLousePosition()
	player.position = target + Vector2(8, 8)
	player.velocity = Vector2.ZERO
	await frames(3)
	if not player.IsDead or player.IsInvincible or world.TestPoints() < 150:
		fail("Louse did not kill vulnerable player")
		return
	print("PASS: louse spawning, acceleration, blaster and lethal collision")
	world.queue_free()
	await process_frame
	quit(0)
