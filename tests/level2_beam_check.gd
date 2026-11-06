extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L02S11.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	player.position = Vector2(270, 272)
	await frames(5)
	for i in range(50):
		Input.action_press("fire")
		await frames(1)
		Input.action_release("fire")
		await frames(6)
		if i == 48 and not world.TestObstacleAlive("beam"):
			push_error("Beam broke before 50 blaster hits")
			quit(1)
			return
	if world.TestObstacleAlive("beam") or world.TestAwardRemaining() + world.TestPoints() < 1000:
		push_error("Beam did not break and award 1000 points after 50 hits")
		quit(1)
		return
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L02S11.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.position = Vector2(280, 272)
	Input.action_press("ui_right")
	await frames(60)
	Input.action_release("ui_right")
	if not player.IsDead:
		push_error("Walking into the beam did not kill the player")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://level2_zone11_check.png")
	print("PASS: beam takes 50 blaster hits, awards points, and kills on contact")
	world.queue_free()
	await process_frame
	quit(0)
