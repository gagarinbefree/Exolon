extends SceneTree

func _initialize(): call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func run_check():
	var world = load("res://scenes/zones/L03S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	player.position = Vector2(304, 256)
	player.velocity = Vector2.ZERO
	await frames(5)
	if not world.TestLevelCompleteVisible(): return fail("Final exit did not open completion")
	await frames(240)
	if world.TestPoints() != 19000: return fail("Final completion bonus differs from reference")
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(5)
	if not world.TestBonusScreenVisible(): return fail("Final bonus screen did not open")
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(75)
	if world.TestZone() != 75 or world.TestNextLevel() != "L01S02": return fail("Final bonus did not return to first map")
	print("PASS: final completion, bonus and return to L01S01")
	world.queue_free()
	await process_frame
	quit(0)

func fail(message: String):
	push_error(message)
	quit(1)
