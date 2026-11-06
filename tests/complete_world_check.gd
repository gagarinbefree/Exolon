extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L02S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(30.0)
	for i in range(80):
		await physics_frame
		if world.TestBubbleCount() > 0: break
	if world.TestBubbleCount() == 0: return fail("Bubble did not spawn")
	player.position = Vector2(384, 208)
	player.velocity = Vector2.ZERO
	for i in range(5): await physics_frame
	if not world.TestLevelCompleteVisible(): return fail("Completion window did not open")
	var before: Vector2 = world.TestFirstBubblePosition()
	for i in range(45): await physics_frame
	var after: Vector2 = world.TestFirstBubblePosition()
	if absf(before.x - after.x - 135.0) > 0.1 or player.IsDead or world.TestBubbleCount() != 1:
		return fail("Bubble stopped or collided with the removed player during completion")
	print("PASS: enemies continue moving behind completion window without hitting removed player")
	world.queue_free()
	await process_frame
	quit(0)

func fail(message: String):
	push_error(message)
	quit(1)
