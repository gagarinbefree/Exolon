extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func send_touch(index: int, position: Vector2, pressed: bool) -> void:
	var event := InputEventScreenTouch.new()
	event.index = index
	event.position = position
	event.pressed = pressed
	Input.parse_input_event(event)

func run_check() -> void:
	var title = load("res://scenes/screens/title_screen.tscn").instantiate()
	root.add_child(title)
	current_scene = title
	var controls = root.get_node("AndroidTouch")
	controls.TestEnable()
	await process_frame
	var size := root.get_visible_rect().size
	var diagonal: Vector2
	var fire: Vector2
	if size.x > size.y:
		var screen_scale := maxf(0.5, DisplayServer.screen_get_scale()) if OS.has_feature("android") else 1.0
		var tablet := size.x / size.y <= 1.6
		var gap := 20.0 * screen_scale
		var game_min := minf(256.0 * screen_scale, maxf(0.0, (size.x - 2.0 * gap) / 3.0))
		var control_min := minf((400.0 if tablet else 160.0) * screen_scale, maxf(0.0, size.x - 2.0 * gap - game_min))
		var available := maxf(0.0, size.x - control_min - 2.0 * gap)
		var game_width := minf(available, size.y * 4.0 / 3.0)
		var controls_width := size.x - game_width - 2.0 * gap
		var fire_width := controls_width * 0.3
		var control_width := controls_width * 0.7
		var transform := root.canvas_transform
		if absf(transform.x.x * 512.0 - game_width) > 1.0 or absf(transform.origin.x - fire_width - gap) > 1.0:
			push_error("Landscape game viewport did not preserve the control areas")
			quit(1)
			return
		diagonal = Vector2(size.x - control_width + control_width / 6.0, size.y * .2)
		fire = Vector2(fire_width / 2.0, size.y * .5)
	else:
		var controls_top := size.y * .5 + size.x * .375 + 20.0 * minf(size.x, size.y) / 390.0
		diagonal = Vector2(size.x * .35, controls_top + (size.y - controls_top) * .2)
		fire = Vector2(size.x * .15, controls_top + (size.y - controls_top) * .5)
	send_touch(0, diagonal, true)
	send_touch(1, fire, true)
	await process_frame
	if not Input.is_action_pressed("ui_up") or not Input.is_action_pressed("ui_left") or not Input.is_action_pressed("fire"):
		push_error("Android diagonal and fire were not pressed together")
		quit(1)
		return
	for i in range(3): await process_frame
	if current_scene == title or not current_scene.has_node("Player"):
		push_error("Touching fire did not start gameplay")
		quit(1)
		return
	send_touch(0, diagonal, false)
	await process_frame
	if Input.is_action_pressed("ui_up") or Input.is_action_pressed("ui_left") or not Input.is_action_pressed("fire"):
		push_error("Releasing movement affected fire or left directions pressed")
		quit(1)
		return
	send_touch(1, fire, false)
	await process_frame
	if Input.is_action_pressed("fire"):
		push_error("Fire remained pressed")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://artifacts"))
		var orientation := "landscape" if size.x > size.y else "portrait"
		root.get_texture().get_image().save_png("res://artifacts/android-touch-" + orientation + ".png")
	print("PASS: Android-only diagonal and fire support independent simultaneous touches")
	quit(0)
