extends SceneTree

# Keep the real title screen and gameplay, but remove random deaths from the
# browser input test and expose the existing ammunition test accessor.
func _initialize() -> void:
	if not OS.has_feature("web"):
		quit()
	else:
		call_deferred("run_fixture")

func run_fixture() -> void:
	var title = load("res://scenes/screens/title_screen.tscn").instantiate()
	root.add_child(title)
	current_scene = title
	while current_scene == null or current_scene.name != "FirstScreen":
		await process_frame
	var world = current_scene
	var player = world.get_node("Player")
	player.GrantInvulnerability(60.0)
	while Input.is_action_pressed("fire"):
		await physics_frame
	# Let gameplay observe the title-screen key release before the next press.
	for i in range(2): await physics_frame
	JavaScriptBridge.eval("window.exolonTouchReady = true", true)
	while is_instance_valid(world) and current_scene == world:
		JavaScriptBridge.eval("window.exolonTouchAmmo = %d" % world.TestAmmo(), true)
		await physics_frame
