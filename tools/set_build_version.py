"""Stamp the same version into the game and web update metadata."""
import json
import pathlib
import re
import sys

version = sys.argv[1]
if not re.fullmatch(r"(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)", version):
    raise SystemExit("Version must use X.Y.Z")
project = pathlib.Path("project.godot")
content, count = re.subn(r'(?m)^config/version="[^"]*"$', f'config/version="{version}"', project.read_text(encoding="utf-8"))
if count != 1:
    raise SystemExit("Could not find project version")
project.write_text(content, encoding="utf-8", newline="\n")
pathlib.Path("Exolon.web/wwwroot/version.json").write_text(json.dumps({"version": version}) + "\n", encoding="utf-8")
