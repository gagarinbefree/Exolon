import base64
import glob
import struct
import xml.etree.ElementTree as ET

for path in glob.glob('assets/maps/L*.tmx'):
    root = ET.parse(path).getroot()
    spawn = next(item for item in root.iter('object') if item.get('name') == 'vitorc')
    x, y = int(spawn.get('x')), int(spawn.get('y'))
    layer = next(item for item in root.findall('layer') if item.get('name') == 'Collision')
    width, height = int(layer.get('width')), int(layer.get('height'))
    tiles = struct.unpack(f'<{width * height}I', base64.b64decode(layer.findtext('data')))
    floor = next((row for row in range(y, height * 16)
                  if any(0 <= (x + dx) // 16 < width and tiles[row // 16 * width + (x + dx) // 16]
                         for dx in (0, 16, 32))), None)
    if floor is None or floor != y:
        print(path, 'spawn bottom:', y, 'floor:', floor)
