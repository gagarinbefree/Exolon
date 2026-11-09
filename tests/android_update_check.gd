extends SceneTree

func _initialize() -> void:
	call_deferred("run_check")

func run_check() -> void:
	var title = load("res://scenes/screens/title_screen.tscn").instantiate()
	root.add_child(title)
	var expected := "https://github.com/gagarinbefree/Exolon/releases/download/v0.1.28/Exolon-0.1.28.apk"
	var release := JSON.stringify({"tag_name": "v0.1.28", "assets": [
		{"name": "Exolon-0.1.28.apk", "browser_download_url": expected}]})
	if title.TestAndroidApkUrl(release, "0.1.28") != expected:
		push_error("Could not select the versioned Android APK")
		quit(1)
		return
	for bad in [
		JSON.stringify({"tag_name": "v0.1.28", "assets": []}),
		JSON.stringify({"tag_name": "v0.1.28", "assets": [{"name": "Exolon-0.1.28.apk", "browser_download_url": "https://example.com/file.apk"}]}),
		JSON.stringify({"tag_name": "v0.1.29", "assets": [{"name": "Exolon-0.1.28.apk", "browser_download_url": expected}]})
	]:
		if title.TestAndroidApkUrl(bad, "0.1.28") != "":
			push_error("Accepted a missing, incorrect or untrusted Android APK")
			quit(1)
			return
	print("PASS: Android updates select only the matching release APK")
	quit()
