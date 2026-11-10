"""Download only the verified Windows x64 release template from Godot's .NET TPZ."""
import argparse
import pathlib
import struct
import sys
import tempfile
import time
import urllib.request
import zlib

URL = "https://github.com/godotengine/godot-builds/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_export_templates.tpz"
NAME = b"templates/windows_release_x86_64.exe"
START, END = 1_164_599_829, 1_202_595_640
COMPRESSED_SIZE, SIZE, CRC32 = 37_995_718, 109_513_728, 0xABB825FA
ATTEMPTS, TIMEOUT = 4, 120


def verified(path):
    if not path.is_file() or path.stat().st_size != SIZE:
        return False
    crc = 0
    with path.open("rb") as source:
        while chunk := source.read(1024 * 1024):
            crc = zlib.crc32(chunk, crc)
    return crc == CRC32


def extract(source, destination):
    header = source.read(30)
    if len(header) != 30:
        raise ValueError("Truncated template header")
    values = struct.unpack("<4s5H3L2H", header)
    if values[0] != b"PK\x03\x04" or values[3] != 8:
        raise ValueError("Unexpected template ZIP entry")
    name = source.read(values[-2])
    if len(source.read(values[-1])) != values[-1] or name != NAME:
        raise ValueError("Unexpected template ZIP entry name or extra data")
    crc, size, remaining = 0, 0, COMPRESSED_SIZE
    decoder = zlib.decompressobj(-15)
    with destination.open("wb") as output:
        while remaining:
            chunk = source.read(min(1024 * 1024, remaining))
            if not chunk:
                raise ValueError("Truncated template ZIP entry")
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
        raise ValueError("Template ZIP entry failed its size or CRC check")


def download(destination):
    if verified(destination):
        print(f"Using verified cached Windows template: {destination}")
        return
    destination.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="windows-template-", dir=destination.parent) as directory:
        temporary = pathlib.Path(directory) / "windows_release_x86_64.exe"
        for attempt in range(1, ATTEMPTS + 1):
            try:
                request = urllib.request.Request(URL, headers={
                    "Range": f"bytes={START}-{END}", "User-Agent": "Exolon-Windows-build"
                })
                print(f"Downloading Windows template: attempt {attempt}/{ATTEMPTS}", flush=True)
                with urllib.request.urlopen(request, timeout=TIMEOUT) as source:
                    if source.status != 206 or source.headers.get("Content-Range", "").split("/")[0] != f"bytes {START}-{END}":
                        raise ValueError("The server did not return the expected archive range")
                    extract(source, temporary)
                temporary.replace(destination)
                print(f"Extracted verified Godot 4.7.2 Windows release template: {destination}")
                return
            except (OSError, ValueError, zlib.error) as error:
                temporary.unlink(missing_ok=True)
                if attempt == ATTEMPTS:
                    raise RuntimeError(f"Windows template download failed after {ATTEMPTS} attempt(s): {error}") from error
                delay = min(5 * 2 ** (attempt - 1), 30)
                print(f"Attempt {attempt} failed: {error}; retrying in {delay}s", file=sys.stderr, flush=True)
                time.sleep(delay)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("destination", type=pathlib.Path)
    download(parser.parse_args().destination)
