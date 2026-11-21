"""Prepare a one-time conversion manifest; the game never reads this file."""
import base64
import json
import pathlib
import struct
import xml.etree.ElementTree as ET

root = pathlib.Path(__file__).resolve().parent.parent
maps = []
for path in sorted((root / 'assets/maps').glob('L??S??.tmx')):
    source = ET.parse(path).getroot()
    def props(element):
        return {p.get('name'): p.get('value', '') for p in element.findall('./properties/property')}
    sets = []
    for item in source.findall('tileset'):
        image = item.find('image')
        sets.append(dict(name=item.get('name'), firstgid=int(item.get('firstgid')),
                         width=int(item.get('tilewidth')), height=int(item.get('tileheight')),
                         image_width=int(image.get('width')), image_height=int(image.get('height'))))
    layers = []
    for element in source:
        if element.tag not in ('layer', 'objectgroup', 'imagelayer'):
            continue
        layer = dict(kind=element.tag, name=element.get('name', ''), visible=element.get('visible', '1') != '0')
        if element.tag == 'layer':
            layer['width'] = int(element.get('width'))
            raw = base64.b64decode(element.findtext('data'))
            layer['cells'] = [list((i % layer['width'], i // layer['width'], gid)) for i, (gid,) in enumerate(struct.iter_unpack('<I', raw)) if gid]
        elif element.tag == 'objectgroup':
            layer['objects'] = [dict(name=o.get('name', ''), x=int(float(o.get('x', '0'))), y=int(float(o.get('y', '0'))),
                                     gid=int(o.get('gid', '0')), width=int(float(o.get('width', '0'))), height=int(float(o.get('height', '0'))),
                                     properties=props(o)) for o in element.findall('object')]
        layers.append(layer)
    maps.append(dict(id=path.stem, next_level=props(source).get('nextLevel', ''), tilesets=sets, layers=layers))
if len(maps) != 75:
    raise SystemExit(f'Expected 75 maps, found {len(maps)}')
target = root / 'artifacts/map_conversion.json'
target.parent.mkdir(exist_ok=True)
target.write_text(json.dumps(maps, separators=(',', ':')), encoding='utf-8')
print(f'Prepared {len(maps)} maps in {target}')
