extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L01S03.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	if world.TestObjectZ("teleport", 64, 288) <= player.z_index or world.TestObjectZ("turret", 224, 288) >= player.z_index or world.TestObjectZ("piston", 432, 288) >= player.z_index:
		push_error("Object order in zone 3 differs from melonJS")
		quit(1)
		return
	player.GrantInvulnerability(30.0)
	player.position = Vector2(96, 256)
	for i in range(5): await physics_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("user://layer_overlap_check.png")
	print("PASS: teleport is in front of player; turret and piston follow their TMX order")
	world.queue_free()
	await process_frame
	quit()
