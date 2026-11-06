extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S11.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for i in range(120):
		await physics_frame
		if world.TestInterceptorCount() > 0: break
	var first: Vector2 = world.TestFirstInterceptorPosition()
	if first.x < 500 or first.x > 544:
		fail("Interceptor did not spawn at right edge: " + str(first))
		return
	await frames(5)
	var second: Vector2 = world.TestFirstInterceptorPosition()
	if absf(first.x - second.x - 7.5) > 0.1:
		fail("Interceptor normal speed differed from melonJS")
		return
	for i in range(240):
		await physics_frame
		if world.TestFirstInterceptorFast(): break
	if not world.TestFirstInterceptorFast():
		fail("Interceptor did not accelerate")
		return
	first = world.TestFirstInterceptorPosition()
	await frames(5)
	second = world.TestFirstInterceptorPosition()
	if absf(first.x - second.x - 20) > 0.1:
		fail("Interceptor fast speed differed from melonJS")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone11_check.png")
	world.queue_free()
	await process_frame
	# Use zone 12 to isolate blaster collision from zone 11's upper rockets.
	world = load("res://scenes/zones/L01S12.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for i in range(500):
		await physics_frame
		if world.TestInterceptorCount() > 0 and world.TestFirstInterceptorPosition().x < 350: break
	var target: Vector2 = world.TestFirstInterceptorPosition()
	var before: int = world.TestPoints()
	player.position = target + Vector2(-40, 4)
	player.velocity = Vector2.ZERO
	Input.action_press("fire")
	await frames(6)
	Input.action_release("fire")
	if world.TestPoints() < before + 150:
		fail("Blaster did not destroy interceptor for 150 points")
		return
	world.queue_free()
	await process_frame
	# Isolate contact with a vulnerable player in a fresh zone.
	world = load("res://scenes/zones/L01S12.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	for i in range(500):
		await physics_frame
		if world.TestInterceptorCount() > 0 and world.TestFirstInterceptorPosition().x < 350: break
	target = world.TestFirstInterceptorPosition()
	player.position = target + Vector2(8, 8)
	player.velocity = Vector2.ZERO
	await frames(3)
	if not player.IsDead or player.IsInvincible or world.TestPoints() < 150:
		fail("Interceptor did not kill a vulnerable player")
		return
	print("PASS: interceptor spawning, acceleration, blaster hit and lethal collision")
	world.queue_free()
	await process_frame
	quit(0)
