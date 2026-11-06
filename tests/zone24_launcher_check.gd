extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S24.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(35.0)
	await frames(3)
	if world.TestPistonCount() != 1 or world.TestObstacleCount("waggon") != 1 or world.TestObstacleCount("rocket") != 1 or world.TestObstacleCount("combined_launcher_top") != 1 or world.TestLauncherCount() != 1:
		fail("Zone 24 objects missing")
		return
	var top_shot := false
	var bottom_shot := false
	for i in range(330):
		await physics_frame
		if world.TestTurretBulletsAtY(246) > 0: top_shot = true
		var pos: Vector2 = world.TestLauncherBulletPosition()
		if pos.x >= 0 and absf(pos.y - 272) < 0.1: bottom_shot = true
		if top_shot and bottom_shot: break
	if not top_shot or not bottom_shot:
		fail("Both combined launcher parts did not fire from expected heights")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone24_check.png")
	# Position as in the browser capture to isolate the bonus.
	player.position = Vector2(404, 272)
	player.velocity = Vector2.ZERO
	await frames(5)
	if not world.TestLauncherCaptured() or world.TestAwardRemaining() <= 0:
		fail("Combined launcher capture did not start")
		return
	await frames(365)
	if world.TestPoints() != 3000 or world.TestAwardRemaining() != 0:
		fail("Combined launcher bonus should total 3000")
		return
	player.position = Vector2(392, 272)
	player.velocity = Vector2.ZERO
	await frames(3)
	Input.action_press("ui_down")
	await frames(3)
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	Input.action_release("ui_down")
	for i in range(120):
		await physics_frame
		if not world.TestObstacleAlive("combined_launcher_top"): break
	print("Combined top: alive=", world.TestObstacleAlive("combined_launcher_top"), " points=", world.TestPoints(), " player=", player.position, " grenade=", world.TestGrenadePosition())
	if world.TestObstacleAlive("combined_launcher_top") or world.TestPoints() != 3150:
		fail("Combined launcher top was not destroyed for 150 points")
		quit(1)
		return
	print("PASS: combined launcher firing, capture bonus and destructible top")
	world.queue_free()
	await process_frame
	quit(0)
