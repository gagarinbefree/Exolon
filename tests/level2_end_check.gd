extends SceneTree

func _initialize(): call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L02S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	for i in range(50):
		await physics_frame
		if world.TestBubbleCount() > 0: break
	if world.TestBubbleCount() == 0:
		push_error("Bubble did not spawn with the configured delay")
		quit(1)
		return
	var first: Vector2 = world.TestFirstBubblePosition()
	await frames(5)
	var second: Vector2 = world.TestFirstBubblePosition()
	if absf(first.x - second.x - 15.0) > 0.1 or absf(first.y - second.y) > 0.1:
		push_error("Bubble did not use circular behavior's straight phase")
		quit(1)
		return
	for i in range(120):
		await physics_frame
		if world.TestFirstBubbleCircular(): break
	if not world.TestFirstBubbleCircular():
		push_error("Bubble did not enter its circular phase")
		quit(1)
		return
	player.position = Vector2(384, 208)
	player.velocity = Vector2.ZERO
	await frames(5)
	if not world.TestLevelCompleteVisible():
		push_error("Second level exit did not open the completion screen")
		quit(1)
		return
	await frames(240)
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(5)
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(75)
	if world.TestZone() != 50 or world.TestNextLevel() != "L03S02" or world.TestObstacleCount("beam") != 1:
		push_error("Second level bonus did not advance to the third level")
		quit(1)
		return
	print("PASS: level 2 circular bubble, completion and transition to level 3")
	world.queue_free()
	await process_frame
	quit(0)
