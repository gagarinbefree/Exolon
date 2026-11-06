extends SceneTree

func _initialize():
	call_deferred("check_animations")

func check_animations():
	var world = load("res://scenes/screens/first_screen.tscn").instantiate()
	root.add_child(world)
	var light_frames = {}
	var fire_frames = {}
	var fire_sizes = {}
	var light_images = {}
	var fire_images = {}
	for i in range(30):
		await create_timer(0.055).timeout
		await RenderingServer.frame_post_draw
		var light: int = world.TestLightFrame()
		var fire: int = world.TestShipFireFrame(16, 144)
		light_frames[light] = true
		fire_frames[fire] = true
		fire_sizes[fire % 3] = true
		var image := root.get_texture().get_image()
		light_images[image.get_region(Rect2i(64, 176, 48, 16)).get_data().hex_encode()] = true
		fire_images[image.get_region(Rect2i(16, 112, 16, 32)).get_data().hex_encode()] = true
	print("Ambient frames: lights=", light_frames.size(), " fire=", fire_frames.size(), " fire sizes=", fire_sizes.size(), " rendered light=", light_images.size(), " fire=", fire_images.size())
	if light_frames.size() < 6 or fire_frames.size() < 4 or fire_sizes.size() < 3 or light_images.size() < 3 or fire_images.size() < 3:
		push_error("Ambient animation frames or rendered sprites did not change")
		quit(1)
		return
	print("PASS: ship flames and portal lights animate")
	quit(0)
