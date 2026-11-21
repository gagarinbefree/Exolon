extends Node2D

var data: Dictionary
var textures := {}

func setup(entry: Dictionary) -> void:
	data = entry
	for item in data.tilesets:
		textures[item.name] = load("res://assets/images/%s.png" % item.name)
	for name in ["vitorc2", "turret_tube", "egg"]:
		textures[name] = load("res://assets/images/%s.png" % name)

func _draw() -> void:
	draw_rect(Rect2(0, 0, 512, 352), Color.BLACK)
	var random := RandomNumberGenerator.new()
	random.seed = 1987
	for i in 50:
		var x := random.randi_range(0, 31) * 16
		var y := random.randi_range(0, 17) * 16
		var gid := random.randi_range(10, 14)
		_draw_gid(x, y, gid)
	for layer in data.layers:
		if not layer.visible:
			continue
		if layer.kind == "layer" and layer.name not in ["Stars", "Collision"]:
			for cell in layer.cells:
				_draw_gid(int(cell[0]) * 16, int(cell[1]) * 16, int(cell[2]))
		elif layer.kind == "objectgroup":
			for index in range(layer.objects.size() - 1, -1, -1):
				_draw_object(layer.objects[index])

func _draw_object(object: Dictionary) -> void:
	var x := int(object.x)
	var y := int(object.y)
	if object.name == "vitorc":
		_draw_frame("vitorc2", x, y - 64, 48, 64)
		return
	if int(object.gid) > 0:
		_draw_gid(x, y - 16, int(object.gid))
	if object.name == "combined_launcher_top":
		_draw_frame("turret_tube", x - 16, y - 32, 32, 16)
	elif object.name == "incubator":
		_draw_frame("egg", x + 20, y - 76, 16, 16)

func _draw_frame(name: String, x: int, y: int, width: int, height: int) -> void:
	draw_texture_rect_region(textures[name], Rect2(x, y, width, height), Rect2(0, 0, width, height))

func _draw_gid(x: int, y: int, gid: int) -> void:
	var chosen: Dictionary = {}
	for info in data.tilesets:
		if int(info.firstgid) <= gid:
			chosen = info
	if chosen.is_empty():
		return
	var texture: Texture2D = textures[chosen.name]
	var width := int(chosen.width)
	var height := int(chosen.height)
	var columns: int = texture.get_width() / width
	var frame := gid - int(chosen.firstgid)
	draw_texture_rect_region(texture, Rect2(x, y + 16 - height, width, height), Rect2((frame % columns) * width, (frame / columns) * height, width, height))
