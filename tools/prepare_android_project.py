"""Restore Godot's Android target override immediately before exporting.

The 2dog web importer currently removes conditional TargetFramework entries
from the project file. Keep the web default and apply the official Android
template's .NET 9 target only for the duration of an Android build.
"""
import pathlib

path = pathlib.Path("Exolon.csproj")
content = path.read_text(encoding="utf-8")
override = '<TargetFramework Condition="\'$(GodotTargetPlatform)\' == \'android\'">net9.0</TargetFramework>'
if override not in content:
    default = '<TargetFramework>net10.0</TargetFramework>'
    if content.count(default) != 1:
        raise SystemExit("Expected exactly one .NET 10 default target in Exolon.csproj")
    content = content.replace(default, default + "\n    " + override, 1)
    path.write_text(content, encoding="utf-8", newline="\n")
print("Android export configured for .NET 9")
