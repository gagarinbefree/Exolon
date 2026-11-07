"""Set the Android export version from an X.Y.Z version string for a CI build."""
import pathlib
import re
import sys

version = sys.argv[1]
match = re.fullmatch(r"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)", version)
if not match:
    raise SystemExit("Android version must use X.Y.Z (for example 0.1.0)")
major, minor, patch = map(int, match.groups())
if minor > 999 or patch > 999 or major > 2099:
    raise SystemExit("Android version exceeds the supported version-code range")
code = major * 1_000_000 + minor * 1_000 + patch
if code < 1 or code > 2_100_000_000:
    raise SystemExit("Android version code must be between 1 and 2100000000")
path = pathlib.Path("export_presets.cfg")
content = path.read_text(encoding="utf-8")
before, section = content.split('[preset.4.options]', 1)
android, after = section.split('[preset.5]', 1)
android, names = re.subn(r'(?m)^version/name="[^"]*"$', f'version/name="{version}"', android)
android, codes = re.subn(r'(?m)^version/code=\d+$', f'version/code={code}', android)
if names != 1 or codes != 1:
    raise SystemExit("Could not locate Android version options")
path.write_text(before + '[preset.4.options]' + android + '[preset.5]' + after, encoding="utf-8", newline="\n")
print(f"Android version {version}, version code {code}")
