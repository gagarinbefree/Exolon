extends SceneTree

func _initialize(): call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L01S01.tscn").instantiate()
	root.add_child(world)
	world.TestPlaySound("explosion")
	world.TestPlaySound("points")
	if world.TestActiveAudioChannels("explosion") != 1 or world.TestActiveAudioChannels("points") != 1:
		push_error("Different sound effects interrupted each other")
		quit(1)
		return
	for i in range(4): world.TestPlaySound("shot2")
	if world.TestActiveAudioChannels("shot2") != 3:
		push_error("Blaster sound did not use its three reference channels")
		quit(1)
		return
	print("PASS: different effects overlap and blaster uses three channels")
	world.queue_free()
	await process_frame
	quit(0)
