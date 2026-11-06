extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func wait_for_jelly(world):
	for i in range(100):
		await physics_frame
		if world.TestJellyfishCount() > 0: return

func run_check():
	var world = load("res://scenes/zones/L01S16.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await wait_for_jelly(world)
	var first: Vector2 = world.TestFirstJellyfishPosition()
	await frames(5)
	var second: Vector2 = world.TestFirstJellyfishPosition()
	if first.x < 500 or absf(first.x - second.x - 15) > 0.1 or absf(first.y - second.y) > 0.1:
		fail("Jellyfish did not move straight at the original speed")
		return
	for i in range(140):
		await physics_frame
		if world.TestFirstJellyfishCircular(): break
	if not world.TestFirstJellyfishCircular():
		fail("Jellyfish did not enter circular motion")
		return
	first = world.TestFirstJellyfishPosition()
	await frames(12)
	second = world.TestFirstJellyfishPosition()
	if second.y >= first.y - 1:
		fail("Jellyfish did not follow circular trajectory")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone16_check.png")
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S16.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	await wait_for_jelly(world)
	for i in range(100):
		await physics_frame
		if world.TestFirstJellyfishPosition().x < 350: break
	var target: Vector2 = world.TestFirstJellyfishPosition()
	var before: int = world.TestPoints()
	player.position = target + Vector2(-40, 4)
	player.velocity = Vector2.ZERO
	Input.action_press("fire")
	await frames(6)
	Input.action_release("fire")
	if world.TestPoints() < before + 150:
		fail("Blaster did not destroy jellyfish")
		return
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S16.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	await wait_for_jelly(world)
	for i in range(100):
		await physics_frame
		if world.TestFirstJellyfishPosition().x < 350: break
	target = world.TestFirstJellyfishPosition()
	player.position = target + Vector2(8, 8)
	player.velocity = Vector2.ZERO
	await frames(3)
	if not player.IsDead or player.IsInvincible or world.TestPoints() < 150:
		fail("Jellyfish did not kill vulnerable player")
		return
	print("PASS: jellyfish spawning, circular movement, blaster hit and lethal collision")
	world.queue_free()
	await process_frame
	quit(0)
