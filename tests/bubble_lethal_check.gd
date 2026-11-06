extends SceneTree

func _initialize():
	call_deferred("check_lethal")

func check_lethal():
	var world = load("res://scenes/zones/L01S04.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	for i in range(1200):
		await physics_frame
		if player.IsDead: break
	if not player.IsDead:
		push_error("Bubble did not kill player")
		quit(1)
		return
	print("PASS: bubble kills player on collision")
	world.queue_free()
	await process_frame
	quit(0)
