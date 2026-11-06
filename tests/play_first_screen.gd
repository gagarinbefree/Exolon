extends SceneTree

# Test driver only: gameplay remains in C#. Inputs go through Godot's InputMap.
var status_label: Label

func _initialize():
	root.title = "EXOLON - AUTOMATIC DEMONSTRATION"
	root.always_on_top = true
	call_deferred("run_scenario")

func wait_physics(frames: int):
	for i in range(frames):
		await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_scenario():
	while true:
		await run_round()

func run_round():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	var overlay = CanvasLayer.new()
	root.add_child(overlay)
	status_label = Label.new()
	status_label.position = Vector2(6, 4)
	status_label.add_theme_font_size_override("font_size", 18)
	status_label.add_theme_color_override("font_color", Color.WHITE)
	status_label.add_theme_color_override("font_outline_color", Color.BLACK)
	status_label.add_theme_constant_override("outline_size", 5)
	overlay.add_child(status_label)
	paused = true
	status_label.text = "AUTO PLAY: starting in 3 seconds"
	await create_timer(3, true).timeout
	paused = false
	await wait_physics(10)
	var player = world.get_node("Player")
	# The demonstration repeats visibly without a random turret hit interrupting it.
	player.GrantInvulnerability(30.0)
	var start: Vector2 = player.position
	status_label.text = "AUTO TEST: walking right"
	Input.action_press("ui_right")
	await wait_physics(32)
	Input.action_release("ui_right")
	await wait_physics(3)
	var walked: Vector2 = player.position
	print("WALK start=", start, " end=", walked)
	if walked.x - start.x < 35:
		fail("Player did not walk right")
		return
	status_label.text = "AUTO TEST: ducking"
	Input.action_press("ui_down")
	await wait_physics(5)
	var sprite = player.get_child(0)
	print("DUCK frame=", sprite.region_rect.position.x / 48)
	if sprite.region_rect.position.x != 9 * 48:
		fail("Duck animation not displayed")
		return
	Input.action_release("ui_down")
	await wait_physics(3)
	status_label.text = "AUTO TEST: jumping"
	Input.action_press("ui_up")
	await wait_physics(12)
	Input.action_release("ui_up")
	var jumping: Vector2 = player.position
	print("JUMP position=", jumping)
	if jumping.y >= walked.y - 15:
		fail("Player did not jump")
		return
	await wait_physics(60)
	if not player.is_on_floor():
		fail("Player did not land")
		return
	var bodies_before = world.get_children().filter(func(node): return node is StaticBody2D).size()
	status_label.text = "AUTO TEST: firing grenade"
	Input.action_press("fire")
	await wait_physics(100)
	Input.action_release("fire")
	await wait_physics(90)
	var bodies_after = world.get_children().filter(func(node): return node is StaticBody2D).size()
	print("FIRE obstacle bodies before=", bodies_before, " after=", bodies_after)
	if bodies_after >= bodies_before:
		fail("Grenade did not destroy the turret")
		return
	print("PASS: movement, ducking, jump, landing and grenade hit")
	status_label.text = "AUTO PLAY PASSED - REPEATING"
	paused = true
	await create_timer(4, true).timeout
	world.queue_free()
	overlay.queue_free()
	paused = false
	await process_frame
