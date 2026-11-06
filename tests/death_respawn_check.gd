extends SceneTree

func _initialize():
	call_deferred("check_death")

func check_death():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	var died := false
	for i in range(450):
		await physics_frame
		if player.IsDead:
			died = true
			break
	if not died:
		push_error("Turret bullet did not kill player")
		quit(1)
		return
	var grounded_ticks := 0
	for i in range(300):
		await physics_frame
		if player.IsDead and player.is_on_floor(): grounded_ticks += 1
		if not player.IsDead: break
	print("Grounded death ticks=", grounded_ticks, " lives=", world.TestLives(), " ammo=", world.TestAmmo(), " grenades=", world.TestGrenades(), " invincible=", player.IsInvincible)
	if player.IsDead or grounded_ticks < 70 or world.TestLives() != 8 or world.TestAmmo() != 99 or world.TestGrenades() != 10 or not player.IsInvincible:
		push_error("Death or respawn differed from the reference")
		quit(1)
		return
	world.queue_free()
	await process_frame
	print("PASS: turret hit, grounded death delay, respawn and invincibility")
	quit(0)
