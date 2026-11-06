extends SceneTree

func _initialize():
	call_deferred("check_layers")

func screenshot():
	await process_frame
	await RenderingServer.frame_post_draw
	return root.get_texture().get_image()

func check_layers():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	await process_frame
	var player = world.get_node("Player")
	var foreground = world.get_node("Foreground")
	player.position = Vector2(145, 256)
	paused = true
	var with_player = await screenshot()
	player.hide()
	var without_player = await screenshot()
	foreground.hide()
	player.show()
	var player_no_foreground = await screenshot()
	player.hide()
	var empty_no_foreground = await screenshot()
	var covered = 0
	for y in range(224, 288):
		for x in range(121, 169):
			if player_no_foreground.get_pixel(x, y) != empty_no_foreground.get_pixel(x, y) and with_player.get_pixel(x, y) == without_player.get_pixel(x, y):
				covered += 1
	print("Foreground pixels covering player: ", covered)
	if covered == 0:
		push_error("Foreground did not cover player")
		quit(1)
		return
	print("PASS: foreground covers player")
	quit(0)
