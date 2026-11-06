extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(10.0)
	for i in range(5): await physics_frame
	var x0: float = player.position.x
	Input.action_press("ui_right")
	for i in range(30): await physics_frame
	Input.action_release("ui_right")
	var distance: float = player.position.x - x0
	for i in range(3): await physics_frame
	var ground_y: float = player.position.y
	var min_y := ground_y
	Input.action_press("ui_up")
	for i in range(90):
		await physics_frame
		min_y = minf(min_y, player.position.y)
		if i == 8: Input.action_release("ui_up")
		if i > 10 and player.is_on_floor(): break
	var height := ground_y - min_y
	print("Walk 30 frames=", distance, " jump height=", height)
	if absf(distance - 45.0) > 2.0 or absf(height - 36.45) > 1.5 or not player.is_on_floor():
		push_error("Movement differs from melonJS reference")
		quit(1)
		return
	world.queue_free()
	await process_frame
	print("PASS: walk speed, jump height and landing")
	quit(0)
