extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func key_event(code: Key, pressed: bool) -> InputEventKey:
	var event := InputEventKey.new()
	event.physical_keycode = code
	event.pressed = pressed
	return event

func mouse_event(pressed: bool) -> InputEventMouseButton:
	var event := InputEventMouseButton.new()
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = pressed
	return event

func frames(count: int) -> void:
	for i in range(count): await physics_frame

func fail(message: String) -> void:
	push_error(message)
	quit(1)

func run_check() -> void:
	for binding in [[KEY_A, "ui_left"], [KEY_D, "ui_right"], [KEY_W, "ui_up"], [KEY_S, "ui_down"]]:
		if not key_event(binding[0], true).is_action(binding[1]):
			return fail("Missing WASD binding: %s" % binding[1])
	for binding in [[KEY_LEFT, "ui_left"], [KEY_RIGHT, "ui_right"], [KEY_UP, "ui_up"], [KEY_DOWN, "ui_down"]]:
		var event := InputEventKey.new()
		event.keycode = binding[0]
		if not event.is_action(binding[1]):
			return fail("Missing arrow binding: %s" % binding[1])
	if not key_event(KEY_SPACE, true).is_action("fire") or not mouse_event(true).is_action("fire"):
		return fail("Space or left mouse button is not bound to fire")

	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	current_scene = world
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	await frames(20)
	var start_x: float = player.position.x
	Input.parse_input_event(key_event(KEY_D, true))
	await frames(15)
	Input.parse_input_event(key_event(KEY_D, false))
	if player.position.x <= start_x + 5:
		return fail("D did not move the player right")
	start_x = player.position.x
	Input.parse_input_event(key_event(KEY_A, true))
	await frames(15)
	Input.parse_input_event(key_event(KEY_A, false))
	if player.position.x >= start_x - 5:
		return fail("A did not move the player left")
	Input.parse_input_event(key_event(KEY_S, true))
	await frames(5)
	if not player.IsDucking:
		return fail("S did not make the player duck")
	Input.parse_input_event(key_event(KEY_S, false))
	await frames(3)
	var start_y: float = player.position.y
	Input.parse_input_event(key_event(KEY_W, true))
	await frames(5)
	Input.parse_input_event(key_event(KEY_W, false))
	if player.position.y >= start_y - 1:
		return fail("W did not make the player jump")
	for event in [mouse_event(true), key_event(KEY_SPACE, true)]:
		var ammo: int = world.TestAmmo()
		Input.parse_input_event(event)
		await frames(5)
		event.pressed = false
		Input.parse_input_event(event)
		await frames(5)
		if world.TestAmmo() != ammo - 1:
			return fail("Mouse or Space did not consume a round")
	print("PASS: WASD, arrow keys, mouse fire and Space")
	quit(0)
