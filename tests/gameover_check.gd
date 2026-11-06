extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	Engine.time_scale = 4.0
	change_scene_to_file("res://scenes/screens/first_screen.tscn")
	await process_frame
	await process_frame
	var world = current_scene
	var player = world.get_node("Player")
	for death in range(9):
		player.GrantInvulnerability(0.0)
		await physics_frame
		await physics_frame
		if not player.Kill():
			push_error("Could not trigger death " + str(death + 1))
			quit(1)
			return
		for i in range(300):
			await physics_frame
			if not player.IsDead or world.has_node("GameOverWindow"): break
		if death < 8 and (player.IsDead or world.TestLives() != 8 - death):
			push_error("Respawn failed after death " + str(death + 1))
			quit(1)
			return
	if not world.has_node("GameOverWindow") or world.TestLives() != 0:
		push_error("Game-over overlay missing")
		quit(1)
		return
	print("Game over overlay shown after ninth death")
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://gameover_check.png")
	for i in range(400):
		await process_frame
		if current_scene and current_scene.name == "TitleScreen": break
	if not current_scene or current_scene.name != "TitleScreen":
		push_error("Title screen did not open")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://title_check.png")
	Input.action_press("fire")
	await process_frame
	await process_frame
	Input.action_release("fire")
	if not current_scene or current_scene.name != "FirstScreen":
		push_error("Fire did not start a new game")
		quit(1)
		return
	print("PASS: nine deaths, game over, title screen and restart")
	Engine.time_scale = 1.0
	quit(0)
