# Exolon for Godot

[![Test and deploy](https://github.com/gagarinbefree/Exolon/actions/workflows/web.yml/badge.svg)](https://github.com/gagarinbefree/Exolon/actions/workflows/web.yml)
[![Tests](https://img.shields.io/github/check-runs/gagarinbefree/Exolon/main?nameFilter=Test%20Godot%20gameplay&label=tests)](https://github.com/gagarinbefree/Exolon/actions/workflows/web.yml)
[![Latest release](https://img.shields.io/github/v/release/gagarinbefree/Exolon)](https://github.com/gagarinbefree/Exolon/releases/latest)
[![Play in browser](https://img.shields.io/badge/play-in%20browser-blue)](https://gagarinbefree.github.io/Exolon/)

A work-in-progress C# port for Godot 4 .NET, based on the [melonJS Exolon project](https://github.com/newagebegins/exolon). The original game's assets and maps are used for comparison and attribution; see [LICENSE.melonjs](LICENSE.melonjs) for the upstream license.

## Running the game

Open `project.godot` in Godot 4.7.2 .NET, build the C# project with the .NET 10 SDK, and run the main scene. Press Space on the title screen to start. Use the left and right arrows to move, up to jump or use a teleport, down to crouch, and Space to shoot. Hold Space to fire a grenade.

On a touchscreen, use the control areas shown in [the control diagram](docs/virtual_keyboard.txt): fire is on the left and directions are on the right. Tap the fire area to start or shoot; hold it to throw a grenade. The controls are below the game in portrait orientation and on either side in landscape orientation.

All 75 zones are enabled and available as editor previews. Gameplay has been checked with automated scenarios and comparisons against the melonJS version; a complete manual playthrough and further fidelity checks are still needed.

Before release builds, the test workflow runs all non-graphical Godot scenarios in `tests/`, including the one-round automatic gameplay demonstration. Render-dependent screenshot scenarios remain available for local use and are not run in CI. Browser startup, touch controls, and PWA checks run after the browser build.

## Browser version

[Play Exolon in your browser](https://gagarinbefree.github.io/Exolon/).

On Android, use your browser's **Install app** or **Add to Home screen** menu. On iPhone and iPad, open the page, tap **Share**, then **Add to Home Screen**. The installed app opens without browser chrome and requires an internet connection to play.

The browser build runs the same Godot C# game using the experimental [2dog](https://github.com/outfox/2dog) WebAssembly host. It uses the single-threaded Compatibility renderer and does not require cross-origin isolation headers. To build and serve it locally, install the .NET 10 SDK and Python, then run:

```sh
dotnet workload install wasm-tools
dotnet publish Exolon.web --configuration Release
python -m http.server 18765 --directory Exolon.web/AppBundle
```

Open `http://127.0.0.1:18765/` in a WebGL 2-compatible browser. GitHub Actions builds and smoke-tests the browser game; successful pushes to `main` deploy it to GitHub Pages.

## Android

The Android app has its own touch controls; the web controls and desktop keyboard controls remain independent. The [test and deploy workflow](../../actions/workflows/web.yml) builds a signed release APK for every successful push to `main`, using version `0.1.<run number>`. It publishes the APK as both a GitHub Release and the `io.github.gagarinbefree:exolon-android` Maven package in GitHub Packages; it is also available as a workflow artifact. GitHub's Maven registry requires authentication even for public packages.

The workflow requires repository secrets `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEY_ALIAS`, and `ANDROID_KEYSTORE_PASSWORD`. Keep a secure backup of the original release keystore and password: Android requires the same signing key for app updates. Android uses the official Godot 4.7.2 .NET 9 export template; the browser build continues to use .NET 10 and 2dog. Before an Android export, run `python tools/get_android_template.py .godot/android_release.apk` and `python tools/prepare_android_project.py`. The latter restores the Android target override, which the 2dog importer removes when building for the web.

## Project layout

- `assets/images`, `assets/maps`, `assets/sound`: graphics, original TMX maps, and audio.
- `scenes/screens`: shared game screens; `title_screen.tscn` starts the game and `first_screen.tscn` contains gameplay.
- `scenes/zones`: one preview scene for each map.
- `scripts/screens`: map loading and shared screen logic.
- `scripts/entities`: player and game entities implemented in C#.
- `tests`: GDScript scenarios used to check and demonstrate gameplay.
- `tools`: supporting scripts, including the asset importer.
- `docs`: technical notes and comparisons with the original.

Like the melonJS version, the game uses separate TMX files for zones and a shared gameplay screen. Godot does not show the TMX files in its FileSystem dock without an importer: open the corresponding scene in `scenes/zones` to preview a map, or use Tiled to edit the TMX file. Preview objects are generated from the map and are not individually editable in the scene tree.

To reimport assets from the adjacent melonJS checkout on Windows, run `pwsh -File tools/import-melon-assets.ps1`. The `.godot` directory is generated by the editor.
