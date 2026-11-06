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
	Input.action_press("ui_left")
	for i in range(3):
		await physics_frame
	Input.action_release("ui_left")
	Input.action_press("fire")
	var pos = Vector2(-100, -100)
	for i in range(100):
		await physics_frame
		pos = scene.TestGrenadePosition()
		if pos.x > 0:
			break
	Input.action_release("fire")
	if pos.x < 0:
		push_error("Grenade was not launched")
		quit(1)
		return
	for i in range(22):
		await physics_frame
	pos = scene.TestGrenadePosition()
	if pos.x < 0:
		push_error("Grenade vanished before rendering")
		quit(1)
		return
	paused = true
	await process_frame
	await RenderingServer.frame_post_draw
	var screen = root.get_texture().get_image()
	var source = load("res://assets/images/grenade.png").get_image()
	var best_flipped = 0
	var best_normal = 0
	for dy in range(-2, 3):
		for dx in range(-2, 3):
			var flipped = 0
			var normal = 0
			for y in range(16):
				for x in range(16):
					var pixel = screen.get_pixel(int(pos.x) + x + dx, int(pos.y) + y + dy)
					var flip_pixel = source.get_pixel(15 - x, y)
					var normal_pixel = source.get_pixel(x, y)
					if flip_pixel.a > 0.9 and flip_pixel.r + flip_pixel.g + flip_pixel.b > 0.5 and flip_pixel != normal_pixel and pixel.is_equal_approx(flip_pixel):
						flipped += 1
					if normal_pixel.a > 0.9 and normal_pixel.r + normal_pixel.g + normal_pixel.b > 0.5 and flip_pixel != normal_pixel and pixel.is_equal_approx(normal_pixel):
						normal += 1
			best_flipped = maxi(best_flipped, flipped)
			best_normal = maxi(best_normal, normal)
	print("Left-facing grenade pixel matches: flipped=", best_flipped, " normal=", best_normal)
	quit(0 if best_flipped > best_normal else 1)
