"""Extract the official Godot 4.7.2 .NET Android release template using HTTP ranges.

The offsets and CRC are pinned to the official 4.7.2-stable mono TPZ. This avoids
downloading the other platforms in the 1.2 GB archive on every CI run.
"""
import argparse
import pathlib
import struct
import urllib.request
import zlib

URL = "https://github.com/godotengine/godot-builds/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_export_templates.tpz"
START, END = 126_261_236, 230_542_987
COMPRESSED_SIZE, SIZE, CRC32 = 104_281_665, 105_159_173, 0x588903FB

parser = argparse.ArgumentParser()
parser.add_argument("destination", type=pathlib.Path)
parser.add_argument("--range-file", type=pathlib.Path, help="Use an already downloaded range")
args = parser.parse_args()

if args.range_file:
    source = args.range_file.open("rb")
else:
    request = urllib.request.Request(URL, headers={"Range": f"bytes={START}-{END}", "User-Agent": "Exolon-Android-build"})
    source = urllib.request.urlopen(request, timeout=120)
    if source.status != 206 or source.headers.get("Content-Range", "").split("/")[0] != f"bytes {START}-{END}":
        raise SystemExit("The server did not return the expected archive range")

with source:
    header = source.read(30)
    if len(header) != 30:
        raise SystemExit("Truncated template header")
    values = struct.unpack("<4s5H3L2H", header)
    if values[0] != b"PK\x03\x04" or values[3] != 8:
        raise SystemExit("Unexpected template ZIP entry")
    name = source.read(values[-2])
    source.read(values[-1])
    if name != b"templates/android_release.apk":
        raise SystemExit("Unexpected template ZIP entry name")
    args.destination.parent.mkdir(parents=True, exist_ok=True)
    crc, size, remaining = 0, 0, COMPRESSED_SIZE
    decoder = zlib.decompressobj(-15)
    with args.destination.open("wb") as output:
        while remaining:
            chunk = source.read(min(1024 * 1024, remaining))
            if not chunk:
                raise SystemExit("Truncated template ZIP entry")
            remaining -= len(chunk)
            decoded = decoder.decompress(chunk)
            output.write(decoded)
            crc = zlib.crc32(decoded, crc)
            size += len(decoded)
        tail = decoder.flush()
        output.write(tail)
        crc = zlib.crc32(tail, crc)
        size += len(tail)
    if not decoder.eof or size != SIZE or crc != CRC32:
        args.destination.unlink(missing_ok=True)
        raise SystemExit("Template ZIP entry failed its size or CRC check")
print(f"Extracted verified Godot 4.7.2 Android release template: {args.destination}")
