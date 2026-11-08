extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func press(action: String) -> void:
	Input.action_press(action)
	for i in range(3): await physics_frame
	Input.action_release(action)
	for i in range(3): await physics_frame

func run_check() -> void:
	var title = load("res://scenes/screens/title_screen.tscn").instantiate()
	root.add_child(title)
	current_scene = title
	await process_frame
	await press("ui_down")
	await press("fire")
	if current_scene != title:
		push_error("Selecting updates started the game")
		quit(1)
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://artifacts"))
		root.get_texture().get_image().save_png("res://artifacts/update-menu.png")
	await press("fire")
	await press("ui_up")
	await press("fire")
	for i in range(8): await physics_frame
	if current_scene == title or not current_scene.has_node("Player"):
		push_error("Cancel did not return to the main menu: " + str(current_scene.name) + " player=" + str(current_scene.has_node("Player")))
		quit(1)
		return
	print("PASS: update screen cancels back to the main menu and the game starts")
	quit()
