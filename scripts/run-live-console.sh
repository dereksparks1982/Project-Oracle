#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
printf '\033]0;%s\007' 'Project Oracle v0.0.17 - Yala Soar Console'

lock_parent="${XDG_RUNTIME_DIR:-/tmp}"
lock_dir="$lock_parent/project-oracle-v0-0-17.lock"
if ! mkdir "$lock_dir" 2>/dev/null; then
  echo "Project Oracle is already running against the default live world." >&2
  read -r -p "Press Enter to close..." _ || true
  exit 1
fi
trap 'rm -rf -- "$lock_dir"' EXIT

status=0
./scripts/run.sh --terminal-child "$@" </dev/tty || status=$?
echo
[[ "$status" -eq 0 ]] && echo "Project Oracle console ended." || echo "Project Oracle exited with status $status."
if [[ "${PROJECT_ORACLE_KEEP_WINDOW_OPEN:-1}" != "0" ]]; then
  read -r -p "Press Enter to close this Project Oracle window..." _ || true
fi
exit "$status"
