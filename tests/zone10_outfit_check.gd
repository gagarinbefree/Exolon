extends SceneTree

func _initialize():
	call_deferred("run_check")

func frames(n: int):
	for i in range(n): await physics_frame

func fail(message: String):
	push_error(message)
	quit(1)

func run_check():
	var world = load("res://scenes/zones/L01S10.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	await frames(5)
	if world.TestPistonCount() != 2:
		fail("Zone 10 must have two pistons")
		return
	# Isolate the capsule interaction at its original location.
	player.position = Vector2(384, 224)
	player.velocity = Vector2.ZERO
	await frames(5)
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	await frames(3)
	if not player.IsArmored or player.get_child(0).region_rect.position.x != 11 * 48:
		fail("Capsule did not activate the Exolon outfit")
		return
	Input.action_press("fire")
	await frames(2)
	Input.action_release("fire")
	var a: Vector2 = world.TestBlasterBulletPosition(0)
	var b: Vector2 = world.TestBlasterBulletPosition(1)
	print("Armored bullets: ", a, " ", b, " ammo=", world.TestAmmo())
	if world.TestBlasterBulletCount() != 2 or absf(b.y - a.y - 12) > 0.1 or world.TestAmmo() != 98:
		fail("Armored outfit did not fire two bullets for one ammo")
		return
	if DisplayServer.get_name() != "headless":
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("user://zone10_outfit_check.png")
	await frames(5)
	Input.action_press("ui_up")
	await frames(3)
	Input.action_release("ui_up")
	await frames(3)
	if player.IsArmored:
		fail("Capsule did not switch back to Vitorc")
		return
	# Verify independent piston timers and armored immunity to piston contact.
	player.ToggleOutfit()
	player.position = Vector2(88, 288)
	player.velocity = Vector2.ZERO
	var independent := false
	var contacted := false
	for i in range(420):
		await physics_frame
		var p0: float = world.TestPistonRiseAt(0)
		var p1: float = world.TestPistonRiseAt(1)
		if (p0 > 0 and p1 == 0) or (p1 > 0 and p0 == 0): independent = true
		if p1 > 5: contacted = true
		if independent and contacted: break
	if not independent or not contacted or player.IsDead or player.IsInvincible:
		fail("Pistons did not move independently or armor did not protect player")
		return
	# Isolate level transition after testing the armor; outfit must survive it.
	player.position = Vector2(535, 224)
	await frames(6)
	if world.TestZone() != 10 or not player.IsArmored:
		fail("Armored outfit was not carried into zone 11")
		return
	world.queue_free()
	await process_frame
	world = load("res://scenes/zones/L01S08.tscn").instantiate()
	root.add_child(world)
	player = world.get_node("Player")
	player.ToggleOutfit()
	player.position = Vector2(164, 272)
	player.velocity = Vector2.ZERO
	await frames(3)
	if player.IsDead or player.IsInvincible or world.TestMineCount() != 2 or world.TestMineFireCount() != 1:
		fail("Armor did not protect against mine while mine still triggered")
		return
	print("PASS: capsule, dual shots, independent pistons, protection and outfit preservation")
	world.queue_free()
	await process_frame
	quit(0)
