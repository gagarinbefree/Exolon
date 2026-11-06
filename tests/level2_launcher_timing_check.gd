extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L02S24.tscn").instantiate()
	root.add_child(world)
	world.get_node("Player").GrantInvulnerability(30.0)
	if world.TestLauncherCount() != 1 or world.TestLauncherDurationMax(0) != 70:
		push_error("Launcher did not read its timing from the map")
		quit(1)
		return
	for i in range(400):
		await physics_frame
		var delay: int = world.TestLauncherDuration(0)
		if delay < 20 or delay > 70:
			push_error("Launcher delay is outside the original range")
			quit(1)
			return
	if world.TestLauncherShotsFired() < 5:
		push_error("Launcher did not fire at the configured rate")
		quit(1)
		return
	print("PASS: zone 24 launcher uses TMX delay 20-70 frames")
	world.queue_free()
	await process_frame
	quit(0)
