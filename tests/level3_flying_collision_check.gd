extends SceneTree

func _initialize(): call_deferred("run_check")

func await_enemy(world, limit: float):
	for i in range(300):
		await physics_frame
		if world.TestFlyingEnemyCount() > 0 and world.TestFirstFlyingPosition().x < limit: return

func run_check():
	for id in ["L03S02", "L03S05"]:
		var world = load("res://scenes/zones/" + id + ".tscn").instantiate()
		root.add_child(world)
		var player = world.get_node("Player")
		player.GrantInvulnerability(60.0)
		await await_enemy(world, 425)
		if world.TestFlyingEnemyCount() == 0: return fail(id + ": enemy missing")
		var target: Vector2 = world.TestFirstFlyingPosition()
		player.position = target + Vector2(-42, 4)
		player.velocity = Vector2.ZERO
		var before: int = world.TestPoints()
		Input.action_press("fire")
		for i in range(6): await physics_frame
		Input.action_release("fire")
		if world.TestPoints() < before + 150: return fail(id + ": blaster did not award 150 points")
		world.queue_free()
		await process_frame

		world = load("res://scenes/zones/" + id + ".tscn").instantiate()
		root.add_child(world)
		player = world.get_node("Player")
		await await_enemy(world, 410)
		if world.TestFlyingEnemyCount() == 0: return fail(id + ": collision enemy missing")
		target = world.TestFirstFlyingPosition()
		player.position = target + Vector2(8, 8)
		player.velocity = Vector2.ZERO
		for i in range(4): await physics_frame
		if not player.IsDead or world.TestPoints() < 150: return fail(id + ": enemy collision did not kill")
		world.queue_free()
		await process_frame
	print("PASS: flasher and fir are destructible and kill on contact")
	quit(0)

func fail(message: String):
	Input.action_release("fire")
	push_error(message)
	quit(1)
