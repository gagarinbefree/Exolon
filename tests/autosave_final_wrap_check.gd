extends SceneTree

var save_path: String

func _initialize() -> void:
	call_deferred("run_check")

func frames(count: int) -> void:
	for i in count: await physics_frame

func press_fire() -> void:
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(8)

func fail(message: String) -> void:
	push_error(message)
	quit(1)

func run_check() -> void:
	save_path = OS.get_environment("EXOLON_SAVE_PATH")
	if save_path.is_empty():
		save_path = "user://autosave-wrap-test-" + str(Time.get_ticks_usec()) + ".cfg"
		OS.set_environment("EXOLON_SAVE_PATH", save_path)
	var world = load("res://scenes/zones/L03S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	player.position = Vector2(304, 256)
	player.velocity = Vector2.ZERO
	await frames(245)
	if not world.TestLevelCompleteVisible(): return fail("Final completion did not open")
	await press_fire()
	if not world.TestBonusScreenVisible(): return fail("Final bonus did not open")
	await press_fire()
	await frames(75)
	if world.TestZone() != 75: return fail("Final bonus did not begin the next cycle")
	var saved := ConfigFile.new()
	if saved.load(save_path) != OK or saved.get_value("checkpoint", "level") != "L01S01" or saved.get_value("checkpoint", "zone") != 75:
		return fail("Final transition wrote the wrong checkpoint")
	var expected_points: int = world.TestPoints()
	world.queue_free()
	await process_frame
	change_scene_to_file("res://scenes/screens/title_screen.tscn")
	await frames(5)
	await press_fire()
	world = current_scene
	if world.name != "FirstScreen" or world.TestZone() != 75 or world.TestPoints() != expected_points:
		return fail("Title Continue did not restore the first zone after final completion")
	player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	player.position.x = 539
	await frames(8)
	if world.TestZone() != 76 or saved.load(save_path) != OK or saved.get_value("checkpoint", "level") != "L01S02" or saved.get_value("checkpoint", "zone") != 76:
		return fail("Next transition after continuing did not save the cumulative zone")

	# The same validation must work on subsequent cycles and reject inconsistent maps.
	saved.set_value("checkpoint", "zone", 150)
	saved.set_value("checkpoint", "level", "L01S01")
	if saved.save(save_path) != OK: return fail("Could not prepare second-cycle checkpoint")
	change_scene_to_file("res://scenes/screens/title_screen.tscn")
	await frames(5)
	await press_fire()
	if current_scene.name != "FirstScreen" or current_scene.TestZone() != 150:
		return fail("Continue rejected a valid later-cycle checkpoint")
	saved.set_value("checkpoint", "zone", 151)
	if saved.save(save_path) != OK: return fail("Could not prepare invalid checkpoint")
	change_scene_to_file("res://scenes/screens/title_screen.tscn")
	await frames(5)
	await press_fire()
	if current_scene.name != "FirstScreen" or current_scene.TestZone() != 0:
		return fail("An inconsistent zone and map were accepted")
	DirAccess.remove_absolute(ProjectSettings.globalize_path(save_path))
	print("PASS: final checkpoint, Continue, later cycles and invalid map/zone combinations")
	current_scene.queue_free()
	await frames(10)
	OS.delay_msec(300)
	quit(0)
