extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L03S09.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(100.0)
	Engine.time_scale = 10.0
	for i in range(350):
		await physics_frame
		if world.TestHarbringerCount() > 0: break
	Engine.time_scale = 1.0
	if world.TestHarbringerCount() != 1:
		push_error("Harbringer did not spawn after 40 seconds")
		quit(1)
		return
	var before: Vector2 = world.TestFirstHarbringerPosition()
	for i in range(5): await physics_frame
	var after: Vector2 = world.TestFirstHarbringerPosition()
	if absf(before.x - after.x - 15.0) > 0.1 or absf(before.y - after.y) > 0.1:
		push_error("Harbringer movement differs from melonJS")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://harbringer_check.png")
	player.GrantInvulnerability(0.0)
	for i in range(3): await physics_frame
	player.position = world.TestFirstHarbringerPosition() + Vector2(8, 8)
	player.velocity = Vector2.ZERO
	for i in range(3): await physics_frame
	print("Harbringer contact: player=", player.position, " dead=", player.IsDead, " invincible=", player.IsInvincible, " enemies=", world.TestHarbringerCount(), " enemy=", world.TestFirstHarbringerPosition())
	if not player.IsDead or world.TestHarbringerCount() != 0:
		push_error("Harbringer did not kill on contact")
		quit(1)
		return
	print("PASS: harbringer spawns after 40 seconds, moves at original speed and kills on contact")
	world.queue_free()
	await process_frame
	quit(0)
