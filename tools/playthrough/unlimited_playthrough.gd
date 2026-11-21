extends SceneTree

var world: Node
var player: Node
var helper: Node

func _initialize() -> void:
	call_deferred("run")

func run() -> void:
	if OS.get_environment("EXOLON_TEST_UNLIMITED") != "1" or OS.get_environment("EXOLON_SAVE_PATH").is_empty():
		push_error("Test mode and an isolated save are required")
		quit(1)
		return
	change_scene_to_file("res://scenes/screens/title_screen.tscn")
	for i in 5: await physics_frame
	Input.action_press("fire")
	for i in 2: await physics_frame
	Input.action_release("fire")
	for i in 8: await physics_frame
	world = current_scene
	if world == null or not world.has_method("TestZone"):
		push_error("Could not continue the isolated checkpoint")
		quit(1)
		return
	player = world.get_node("Player")
	helper = Node.new()
	helper.set_script(load("res://tools/playthrough/UnlimitedResources.cs"))
	helper.call("Attach", world)
	root.add_child(helper)
	await physics_frame
	await physics_frame
	if world.TestLives() != 9 or world.TestAmmo() != 99 or world.TestGrenades() != 10:
		push_error("Unlimited resource harness did not activate")
		quit(1)
		return
	print("UNLIMITED TEST START zone=", world.TestZone() + 1, " lives=", world.TestLives(), " ammo=", world.TestAmmo(), " grenades=", world.TestGrenades())
	var zone: int = world.TestZone()
	var zone_ticks := 0
	var deaths := 0
	var was_dead := false
	var jump_left := 0
	var last_x: float = player.position.x
	var stuck := 0
	var mode := 0
	for frame in 240000:
		await physics_frame
		if not is_instance_valid(world):
			push_error("World closed unexpectedly")
			quit(1)
			return
		if world.TestZone() != zone:
			zone = world.TestZone()
			print("ZONE ", zone + 1, " deaths=", deaths, " next=", world.TestNextLevel())
			zone_ticks = 0
			stuck = 0
			mode = 0
		if zone >= 75:
			print("COMPLETE: reached zone counter ", zone)
			Input.action_release("ui_right")
			Input.action_release("ui_up")
			Input.action_release("ui_down")
			Input.action_release("fire")
			helper.queue_free()
			world.queue_free()
			await process_frame
			await process_frame
			# The audio mixer stops asynchronously; allow it to release stream playbacks.
			OS.delay_msec(300)
			quit(0)
			return
		if player.IsDead:
			if not was_dead:
				deaths += 1
				mode = (mode + 1) % 5
				print("DEATH zone=", zone + 1, " pos=", player.position, " count=", deaths)
			was_dead = true
			Input.action_release("ui_right")
			Input.action_release("ui_up")
			Input.action_release("ui_down")
			Input.action_release("fire")
			continue
		was_dead = false
		zone_ticks += 1
		if world.TestLevelCompleteVisible():
			Input.action_release("ui_right")
			if zone_ticks % 30 < 3: Input.action_press("fire")
			else: Input.action_release("fire")
			continue
		if zone == 16 and world.TestObstacleAlive("cocoon") and player.position.x >= 265 and player.is_on_floor():
			Input.action_release("ui_right")
			Input.action_release("ui_up")
			Input.action_press("ui_down")
			if zone_ticks % 100 < 43: Input.action_press("fire")
			else: Input.action_release("fire")
			continue
		if zone == 21 and world.TestObstacleCount("rocket") > 0:
			var attack_x: float = 200 if world.TestObstacleCount("rocket") > 1 else 265
			if player.position.x >= attack_x:
				Input.action_release("ui_right")
				Input.action_release("ui_up")
				Input.action_press("ui_down")
				if zone_ticks % 100 < 43: Input.action_press("fire")
				else: Input.action_release("fire")
				continue
		if zone == 26 and world.TestObstacleCount("waggon") > 0:
			var attack_x: float = 25 if world.TestObstacleCount("waggon") > 1 else 100
			if player.position.x >= attack_x:
				Input.action_release("ui_right")
				Input.action_release("ui_up")
				Input.action_press("ui_down")
				if zone_ticks % 100 < 43: Input.action_press("fire")
				else: Input.action_release("fire")
				continue
		Input.action_release("ui_down")
		if absf(player.position.x - last_x) < 0.1 and player.is_on_floor(): stuck += 1
		else: stuck = 0
		last_x = player.position.x
		if stuck > 100:
			mode = (mode + 1) % 5
			stuck = 0
			print("STUCK zone=", zone + 1, " pos=", player.position, " strategy=", mode)
		Input.action_press("ui_right")
		var cycle := zone_ticks % 100
		var rapid: bool = zone in [42, 58] or (zone == 43 and world.TestObstacleCount("fungus") == 0 and world.TestObstacleCount("missile_guidance") == 0) or (zone in [21, 55] and world.TestObstacleCount("rocket") == 0) or (zone == 26 and world.TestObstacleCount("waggon") == 0)
		if (rapid and zone_ticks % 12 < 2) or (not rapid and zone != 21 and cycle < 43): Input.action_press("fire")
		else: Input.action_release("fire")
		if jump_left == 0 and player.is_on_floor() and (stuck > 12 or (zone != 21 and zone_ticks % (55 + mode * 13) == 0)):
			Input.action_press("ui_up")
			jump_left = 18
		if jump_left > 0:
			jump_left -= 1
			if jump_left == 0: Input.action_release("ui_up")
		if zone_ticks % 1800 == 0:
			print("PROGRESS zone=", zone + 1, " pos=", player.position, " deaths=", deaths, " resources=", world.TestLives(), "/", world.TestAmmo(), "/", world.TestGrenades(), " turret=", world.TestObstacleCount("turret"), " cocoon=", world.TestObstacleCount("cocoon"), " rocket=", world.TestObstacleCount("rocket"))
		if zone_ticks > 18000:
			print("BLOCKED zone=", zone + 1, " pos=", player.position, " deaths=", deaths)
			quit(2)
			return
	print("STOP: maximum test frames reached at zone ", zone + 1)
	quit(2)
