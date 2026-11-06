extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var main_path: String = ProjectSettings.get_setting("application/run/main_scene")
	if main_path != "res://scenes/screens/title_screen.tscn": return fail("Title scene is not the main scene")
	change_scene_to_file(main_path)
	for i in range(3): await process_frame
	if not current_scene or current_scene.name != "TitleScreen": return fail("Title screen did not open")
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://title_start_check.png")
	Input.action_press("fire")
	for i in range(10): await physics_frame
	Input.action_release("fire")
	for i in range(3): await physics_frame
	if not current_scene or current_scene.name != "FirstScreen": return fail("Space did not start the game")
	if current_scene.TestAmmo() != 99: return fail("Starting the game also fired the blaster")
	Input.action_press("fire")
	for i in range(3): await physics_frame
	Input.action_release("fire")
	if current_scene.TestAmmo() != 98: return fail("Fire did not work after releasing the start key")
	print("PASS: game opens on the title screen and Space starts without firing")
	quit(0)

func fail(message: String):
	Input.action_release("fire")
	push_error(message)
	quit(1)
