"""Run every Godot scenario in isolation, including render-dependent checks."""
import argparse
import concurrent.futures
import pathlib
import re
import subprocess
import sys
import time

ROOT = pathlib.Path(__file__).resolve().parent.parent
VISUAL = {
    "ambient_animation_check.gd",
    "grenade_direction_check.gd",
    "grenade_trace_check.gd",
    "layer_check.gd",
    "layer_overlap_check.gd",
}

parser = argparse.ArgumentParser()
parser.add_argument("--godot", required=True, help="Path to the Godot .NET editor executable")
parser.add_argument("--group", choices=("logic", "visual"), required=True)
parser.add_argument("--workers", type=int, default=3)
parser.add_argument("--timeout", type=int, default=120, help="Maximum seconds per scenario")
args = parser.parse_args()
if args.workers < 1 or args.timeout < 1:
    parser.error("workers and timeout must be positive")

tests = sorted((ROOT / "tests").glob("*.gd"))
missing = VISUAL - {test.name for test in tests}
if missing:
    raise SystemExit("Missing render-dependent scenarios: " + ", ".join(sorted(missing)))
selected = [test for test in tests if (test.name in VISUAL) == (args.group == "visual")]
if not selected:
    raise SystemExit("No scenarios found")
log_dir = ROOT / "artifacts" / "godot-test-logs"
log_dir.mkdir(parents=True, exist_ok=True)

def run(test):
    command = [args.godot, "--path", str(ROOT), "--fixed-fps", "60", "--audio-driver", "Dummy",
               "--log-file", str(log_dir / (test.stem + ".log"))]
    if args.group == "logic":
        command.append("--headless")
    else:
        command.extend(["--rendering-method", "gl_compatibility"])
    command.extend(["--script", "res://tests/" + test.name, "--", "--ci"])
    start = time.monotonic()
    try:
        result = subprocess.run(command, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                text=True, encoding="utf-8", errors="replace", timeout=args.timeout)
    except subprocess.TimeoutExpired as error:
        return test.name, False, time.monotonic() - start, f"Timed out after {args.timeout}s\n{error.stdout or ''}"
    output = result.stdout
    errors = [line for line in output.splitlines() if "ERROR:" in line and not re.search(r"ERROR: \d+ resources still in use at exit", line)]
    ok = result.returncode == 0 and not errors
    return test.name, ok, time.monotonic() - start, output

failures = []
print(f"Running {len(selected)} {args.group} scenarios from {len(tests)} total", flush=True)
with concurrent.futures.ThreadPoolExecutor(max_workers=args.workers) as executor:
    for future in concurrent.futures.as_completed([executor.submit(run, test) for test in selected]):
        name, ok, duration, output = future.result()
        print(f"{'PASS' if ok else 'FAIL'} {name} ({duration:.1f}s)", flush=True)
        if not ok:
            failures.append((name, output))

for name, output in failures:
    print(f"\n--- {name} ---\n{output}", file=sys.stderr)
print(f"{args.group}: {len(selected) - len(failures)}/{len(selected)} passed", flush=True)
sys.exit(1 if failures else 0)
