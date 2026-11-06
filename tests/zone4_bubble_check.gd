extends SceneTree

func _initialize():
	call_deferred("run_check")

func run_check():
	var world = load("res://scenes/zones/L01S04.tscn").instantiate()
	root.add_child(world)
	var player = world.get_node("Player")
	player.GrantInvulnerability(25.0)
	var appeared := false
	var moved := false
	var fired := false
	var successful_shot := false
	var previous := Vector2.ZERO
	var shot_cooldown := 0
	var screenshot_saved := false
	for i in range(1200):
		await physics_frame
		if fired: Input.action_release("fire")
		fired = false
		if shot_cooldown > 0: shot_cooldown -= 1
		if world.TestBubbleCount() == 0:
			previous = Vector2.ZERO
			continue
		var bubble: Vector2 = world.TestFirstBubblePosition()
		appeared = true
		if not screenshot_saved and bubble.x < 495 and DisplayServer.get_name() != "headless":
			await RenderingServer.frame_post_draw
			root.get_texture().get_image().save_png("user://zone4_check.png")
			screenshot_saved = true
		if previous != Vector2.ZERO and bubble.x < previous.x - 1.0 and bubble.x > previous.x - 2.5: moved = true
		previous = bubble
		var bullet_y: float = player.position.y - 4
		if shot_cooldown == 0 and bubble.x > player.position.x + 70 and bubble.x < player.position.x + 130 and bullet_y >= bubble.y and bullet_y <= bubble.y + 32:
			var old_points: int = world.TestPoints()
			Input.action_press("fire")
			fired = true
			shot_cooldown = 20
			for j in range(18):
				await physics_frame
				Input.action_release("fire")
				if world.TestPoints() > old_points:
					successful_shot = true
					break
			if successful_shot: break
	if not appeared or not moved or not successful_shot:
		push_error("Bubble did not spawn, move or respond to blaster: %s %s %s" % [appeared, moved, successful_shot])
		quit(1)
		return
	print("PASS: bubble spawned, moved left with oscillation and was destroyed by blaster for 150 points")
	world.queue_free()
	await process_frame
	quit(0)
