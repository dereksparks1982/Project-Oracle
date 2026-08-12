#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
live_console="$project_root/scripts/run-live-console.sh"
window_title='Project Oracle v0.0.20 - Yala Soar Console'

fail() { echo "Project Oracle could not open a separate console window: $*" >&2; exit 1; }
[[ -x "$live_console" ]] || fail 'scripts/run-live-console.sh is missing or not executable.'

if [[ "${PROJECT_ORACLE_WINDOW_DRY_RUN:-0}" == "1" ]]; then
  echo "DRY RUN: $window_title"
  exit 0
fi
[[ -n "${DISPLAY:-}${WAYLAND_DISPLAY:-}" ]] || fail 'no graphical display was detected. Run ./scripts/run.sh from a terminal instead.'

if command -v gnome-terminal >/dev/null 2>&1; then
  gnome-terminal -- "$live_console" "$@"
elif command -v ptyxis >/dev/null 2>&1; then
  ptyxis -- "$live_console" "$@"
elif command -v kgx >/dev/null 2>&1; then
  kgx -- "$live_console" "$@"
elif command -v x-terminal-emulator >/dev/null 2>&1; then
  x-terminal-emulator -e "$live_console" "$@"
else
  fail 'no supported terminal app was found.'
fi

echo 'Project Oracle console launched.'
