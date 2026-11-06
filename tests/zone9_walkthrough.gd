extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func move_right_to(player, x: float, limit: int):
	Input.action_press("ui_right")
	for i in range(limit):
		await physics_frame
		if player.position.x >= x: break
	Input.action_release("ui_right")

func fire_grenade():
	Input.action_press("fire")
	await frames(37)
	Input.action_release("fire")
	await frames(105)

func run_check():
	var world = load("res://scenes/zones/L01S09.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(40.0)
	await frames(5)
	await move_right_to(player, 80, 90)
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	await frames(6)
	print("Lower teleport: ", player.position)
	if player.position.x < 130 or player.position.x > 175 or player.position.y < 250:
		push_error("Could not use lower teleport")
		quit(1)
		return
	await move_right_to(player, 260, 160)
	await fire_grenade()
	print("Guidance: alive=", world.TestObstacleAlive("missile_guidance"), " player=", player.position)
	if world.TestObstacleAlive("missile_guidance"):
		push_error("Could not destroy guidance unit")
		quit(1)
		return
	await move_right_to(player, 380, 160)
	await fire_grenade()
	print("Incubator: alive=", world.TestObstacleAlive("incubator"), " eggs=", world.TestEggCount(), " player=", player.position)
	if world.TestObstacleAlive("incubator") or world.TestConfinedEggCount() != 0:
		push_error("Could not destroy incubator")
		quit(1)
		return
	await move_right_to(player, 485, 180)
	await frames(130)
	if player.position.x < 480 or world.TestPoints() < 1150 or world.TestZone() != 8:
		push_error("Did not reach zone 9 boundary with expected points")
		quit(1)
		return
	Input.action_press("ui_right")
	for i in range(90):
		await physics_frame
		if world.TestZone() == 9: break
	Input.action_release("ui_right")
	if world.TestZone() != 9 or world.TestPistonCount() != 2:
		push_error("Transition to zone 10 failed")
		quit(1)
		return
	print("PASS: teleport, guidance, incubator and transition to zone 10")
	world.queue_free()
	await process_frame
	quit(0)
