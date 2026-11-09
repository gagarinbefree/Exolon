extends SceneTree

func _initialize() -> void:
	if not OS.has_feature("web"):
		quit()
	else:
		call_deferred("create_checkpoint")

func create_checkpoint() -> void:
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	current_scene = world
	for i in range(5): await physics_frame
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	player.position.x = 539
	for i in range(60):
		await physics_frame
		if world.TestZone() == 1:
			JavaScriptBridge.eval("window.exolonCheckpointReady = true", true)
			return
	push_error("Web fixture could not enter the second zone")
