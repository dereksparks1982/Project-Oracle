#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

window_title="Project Oracle v0.1.6 - Live Garden Console"
printf '\033]0;%s\007' "$window_title"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Project Oracle needs the .NET 10 SDK. The dotnet command was not found." >&2
  read -r -p "Press Enter to close this Garden console..." _
  exit 1
fi

lock_parent="${XDG_RUNTIME_DIR:-/tmp}"
lock_dir="$lock_parent/project-oracle-live-garden.lock"

if ! mkdir "$lock_dir" 2>/dev/null; then
  echo "Project Oracle is already running in another live Garden console." >&2
  echo "Close the other Garden console before starting a second one against the same save." >&2
  read -r -p "Press Enter to close this Garden console..." _
  exit 1
fi

cleanup() {
  rm -rf -- "$lock_dir"
}
trap cleanup EXIT

status=0
./scripts/run.sh "$@" </dev/tty || status=$?

echo
if [[ "$status" -eq 0 ]]; then
  echo "Project Oracle console ended."
else
  echo "Project Oracle exited with status $status."
fi

if [[ "${PROJECT_ORACLE_KEEP_WINDOW_OPEN:-1}" != "0" ]]; then
  read -r -p "Press Enter to close this Garden console..." _
fi

exit "$status"
