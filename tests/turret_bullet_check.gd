extends SceneTree

func _initialize():
	call_deferred("check")

func check():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	await physics_frame
	world.get_node("Player").GrantInvulnerability(10.0)
	var first = Vector2(-100, -100)
	for i in range(320):
		await physics_frame
		first = world.TestTurretBulletPosition()
		if first.x >= 0:
			break
	if first.x < 0:
		push_error("Turret did not fire")
		quit(1)
		return
	for i in range(5):
		await physics_frame
	var second = world.TestTurretBulletPosition()
	print("Turret bullet first=", first, " after five frames=", second)
	var ok = abs(first.y - 230.0) < 0.01 and abs(second.y - 230.0) < 0.01 and second.x < first.x
	quit(0 if ok else 1)
