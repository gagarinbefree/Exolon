@tool
extends Node2D

const PLAYER_TEXTURE = preload("res://assets/images/vitorc2.png")
const TUBE_TEXTURE = preload("res://assets/images/turret_tube.png")
const EGG_TEXTURE = preload("res://assets/images/egg.png")

## Object kind and behavior consumed by the game when loading this map.
@export var object_type: String = ""
@export var gid: int = 0
@export var size: Vector2i = Vector2i.ZERO
@export var placement: String = "floor"
@export var behavior: String = ""
@export var delay: float = 0.0
@export var fire_duration_min: int = 20
@export var fire_duration_max: int = 160
@export var preview_texture: Texture2D
@export var preview_region: Rect2 = Rect2()

func _draw() -> void:
	if not Engine.is_editor_hint() and not has_meta("preview_for_test"):
		return
	if object_type == "vitorc":
		_draw_frame(PLAYER_TEXTURE, Vector2(0, -64), Vector2(48, 64))
		return
	if preview_texture:
		draw_texture_rect_region(preview_texture, Rect2(0, -preview_region.size.y, preview_region.size.x, preview_region.size.y), preview_region)
	if object_type == "combined_launcher_top":
		_draw_frame(TUBE_TEXTURE, Vector2(-16, -32), Vector2(32, 16))
	elif object_type == "incubator":
		_draw_frame(EGG_TEXTURE, Vector2(20, -76), Vector2(16, 16))

func _draw_frame(texture: Texture2D, offset: Vector2, frame_size: Vector2) -> void:
	draw_texture_rect_region(texture, Rect2(offset, frame_size), Rect2(Vector2.ZERO, frame_size))
