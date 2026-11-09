extends SceneTree

var save_path: String

func _initialize() -> void:
	call_deferred("run_check")

func frames(count: int) -> void:
	for i in range(count): await physics_frame

func press(action: String) -> void:
	Input.action_press(action)
	await frames(3)
	Input.action_release(action)
	await frames(3)

func fail(message: String) -> void:
	push_error(message)
	quit(1)

func run_check() -> void:
	save_path = OS.get_environment("EXOLON_SAVE_PATH")
	if save_path.is_empty():
		save_path = "user://autosave-test-" + str(Time.get_ticks_usec()) + ".cfg"
		OS.set_environment("EXOLON_SAVE_PATH", save_path)
	change_scene_to_file("res://scenes/screens/first_screen.tscn")
	await frames(5)
	var world = current_scene
	var player = world.get_node("Player")
	player.GrantInvulnerability(0.0)
	await frames(2)
	if not player.Kill(): return fail("Could not trigger a life loss")
	for i in range(300):
		await physics_frame
		if not player.IsDead: break
	if player.IsDead or world.TestLives() != 8: return fail("Life loss did not complete")
	player.GrantInvulnerability(60.0)
	player.ToggleOutfit()
	await press("fire")
	if world.TestAmmo() != 98: return fail("Could not change ammunition before saving")
	player.position.x = 539
	await frames(8)
	if world.TestZone() != 1: return fail("Could not reach the second zone")
	var save := ConfigFile.new()
	if save.load(save_path) != OK: return fail("Zone transition did not write a checkpoint")
	if save.get_value("checkpoint", "level") != "L01S02" or save.get_value("checkpoint", "zone") != 1:
		return fail("Checkpoint contains the wrong zone")
	if save.get_value("checkpoint", "ammo") != 98 or save.get_value("checkpoint", "lives") != 8 or not save.get_value("checkpoint", "armored"):
		return fail("Checkpoint did not preserve player state")

	# Leave and continue through the actual title-screen controls.
	change_scene_to_file("res://scenes/screens/title_screen.tscn")
	await frames(5)
	await press("ui_down")
	await press("fire")
	await frames(8)
	if current_scene.name != "FirstScreen": return fail("Continue did not start a game")
	world = current_scene
	player = world.get_node("Player")
	if world.TestZone() != 1 or world.TestAmmo() != 98 or world.TestLives() != 8 or not player.IsArmored:
		return fail("Title continue did not restore the checkpoint")

	# Enter another zone, then continue from the game-over menu.
	player.GrantInvulnerability(60.0)
	player.position.x = 539
	await frames(8)
	if world.TestZone() != 2: return fail("Second checkpoint transition failed")
	if save.load(save_path) != OK or save.get_value("checkpoint", "level") != "L01S03":
		return fail("Second checkpoint did not replace the first")
	change_scene_to_file("res://scenes/screens/game_over_menu.tscn")
	await frames(5)
	await press("ui_down")
	await press("fire")
	await frames(8)
	if current_scene.name != "FirstScreen" or current_scene.TestZone() != 2:
		return fail("Game-over continue did not restore the latest checkpoint")
	if current_scene.TestAmmo() != 98 or current_scene.TestLives() != 8 or not current_scene.get_node("Player").IsArmored:
		return fail("Game-over continue lost player state")

	# Returning to the title must leave the checkpoint available.
	change_scene_to_file("res://scenes/screens/game_over_menu.tscn")
	await frames(5)
	await press("ui_down")
	await press("ui_down")
	await press("fire")
	await frames(8)
	if current_scene.name != "TitleScreen" or save.load(save_path) != OK or save.get_value("checkpoint", "level") != "L01S03":
		return fail("Main menu did not open with the checkpoint intact")

	change_scene_to_file("res://scenes/screens/game_over_menu.tscn")
	await frames(5)
	await press("fire")
	await frames(8)
	if current_scene.name != "FirstScreen" or current_scene.TestZone() != 0 or current_scene.TestAmmo() != 99:
		return fail("Start again did not start a fresh game")
	DirAccess.remove_absolute(ProjectSettings.globalize_path(save_path))
	print("PASS: checkpoints, continue, main menu and starting over")
	quit()
