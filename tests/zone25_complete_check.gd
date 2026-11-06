extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S25.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	await frames(5)
	# Isolate the exit trigger as in the browser reference.
	player.position = Vector2(304, 256)
	player.velocity = Vector2.ZERO
	await frames(4)
	if not world.TestLevelCompleteVisible() or world.TestAwardRemaining() <= 0:
		fail("Level complete window or award missing")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone25_complete_check.png")
	await frames(240)
	if world.TestPoints() != 19000 or world.TestAwardRemaining() != 0:
		fail("Bravery and lives bonus differed from reference: " + str(world.TestPoints()))
		return
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(5)
	if not world.TestBonusScreenVisible():
		fail("Bonus screen did not open")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://bonus_check.png")
	Input.action_press("fire")
	await frames(3)
	Input.action_release("fire")
	await frames(75)
	if world.TestZone() != 25 or world.TestNextLevel() != "L02S02" or world.TestObstacleCount("fungus") != 2 or world.TestLives() != 9 or world.TestAmmo() != 99 or world.TestGrenades() != 10 or player.IsArmored:
		fail("Bonus did not start the next level with restored resources")
		return
	world.queue_free()
	await process_frame
	# Armored outfit must not receive the bravery bonus.
	world = load("res://scenes/zones/L01S25.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.ToggleOutfit()
	player.position = Vector2(304, 256)
	player.velocity = Vector2.ZERO
	await frames(120)
	if not world.TestLevelCompleteVisible() or world.TestPoints() != 9000 or world.TestAwardRemaining() != 0:
		fail("Armored bravery bonus did not match reference")
		return
	print("PASS: exit, bravery/lives bonus, bonus screen, next level and armored bonus")
	world.queue_free()
	await process_frame
	quit(0)
