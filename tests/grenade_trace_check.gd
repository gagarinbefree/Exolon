extends SceneTree

func _initialize():
	call_deferred("check")

func check():
	var scene = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(scene)
	for i in range(8):
		await physics_frame
	var player = scene.get_node("Player")
	player.GrantInvulnerability(10.0)
	player.position = Vector2(280, 256)
	var left = not OS.get_cmdline_user_args().has("--right")
	var action = "ui_left" if left else "ui_right"
	Input.action_press(action)
	for i in range(3):
		await physics_frame
	Input.action_release(action)
	Input.action_press("fire")
	var trace = Vector2(-100, -100)
	for i in range(100):
		await physics_frame
		trace = scene.TestTracePosition()
		if trace.x > 0:
			break
	Input.action_release("fire")
	if trace.x < 0:
		push_error("Grenade trace was not created")
		quit(1)
		return
	var top_left: Vector2 = player.position - Vector2(24, 32)
	var expected_trace = top_left + Vector2(-20 if left else 4, -38)
	if not trace.is_equal_approx(expected_trace):
		push_error("Trace origin mismatch: got %s expected %s" % [trace, expected_trace])
		quit(1)
		return
	for i in range(8):
		await physics_frame
	paused = true
	player.hide()
	var frame: int = scene.TestTraceFrame()
	var grenade: Vector2 = scene.TestGrenadePosition()
	await process_frame
	await RenderingServer.frame_post_draw
	var screen = root.get_texture().get_image()
	var source = load("res://assets/images/grenade_trace.png").get_image()
	var flipped = 0
	var normal = 0
	for y in range(48):
		for x in range(64):
			var p = Vector2(trace.x + x, trace.y + y)
			if Rect2(grenade, Vector2(16, 16)).has_point(p):
				continue
			var pixel = screen.get_pixel(int(p.x), int(p.y))
			var flip_pixel = source.get_pixel(frame * 64 + 63 - x, y)
			var normal_pixel = source.get_pixel(frame * 64 + x, y)
			if flip_pixel == normal_pixel:
				continue
			if flip_pixel.a > 0.9 and flip_pixel.r + flip_pixel.g + flip_pixel.b > 0.5 and pixel.is_equal_approx(flip_pixel):
				flipped += 1
			if normal_pixel.a > 0.9 and normal_pixel.r + normal_pixel.g + normal_pixel.b > 0.5 and pixel.is_equal_approx(normal_pixel):
				normal += 1
	print("Trace direction=", "left" if left else "right", " position=", trace, " frame=", frame, " flipped matches=", flipped, " normal matches=", normal)
	var ok = (flipped > normal and flipped > 0) if left else (normal > flipped and normal > 0)
	quit(0 if ok else 1)
