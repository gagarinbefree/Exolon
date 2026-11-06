extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L01S07.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(20.0)
	var first := Vector2(-100, -100)
	for i in range(180):
		await physics_frame
		first = world.TestLauncherBulletPosition()
		if first.x >= 0: break
	if first.x < 260 or first.x > 272 or (absf(first.y - 272) > 0.1 and absf(first.y - 288) > 0.1):
		push_error("Launcher bullet appeared at wrong position: " + str(first))
		quit(1)
		return
	await frames(5)
	var second: Vector2 = world.TestLauncherBulletPosition()
	if absf(first.x - second.x - 15) > 0.1:
		push_error("Launcher bullet speed mismatch: " + str(first) + " -> " + str(second))
		quit(1)
		return
	# Isolate the stop-firing threshold without yet touching the launcher.
	player.position = Vector2(210, 288)
	player.velocity = Vector2.ZERO
	await frames(3)
	var shots: int = world.TestLauncherShotsFired()
	await frames(180)
	if world.TestLauncherShotsFired() != shots or world.TestLauncherCaptured():
		push_error("Launcher fired with player too close or captured prematurely")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(80):
		await physics_frame
		if world.TestLauncherCaptured(): break
	Input.action_release("ui_right")
	if not world.TestLauncherCaptured() or world.TestPoints() >= 2000 or world.TestAwardRemaining() <= 0:
		push_error("Capture did not start gradual bonus")
		quit(1)
		return
	await frames(245)
	if world.TestPoints() != 2000 or world.TestAwardRemaining() != 0:
		push_error("Capture bonus not fully awarded: " + str(world.TestPoints()))
		quit(1)
		return
	print("PASS: launcher bullet height and speed, near-player stop and gradual capture bonus")
	world.queue_free()
	await process_frame
	quit(0)
