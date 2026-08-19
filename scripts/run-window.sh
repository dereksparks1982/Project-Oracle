#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
executable="$project_root/Project_Oracle_v0_0_23"
window_title='Project Oracle v0.0.23 - Desktop Observatory'

fail() { echo "Project Oracle could not open: $*" >&2; exit 1; }
[[ -x "$executable" ]] || fail 'the validated Project_Oracle_v0_0_23 desktop executable is missing.'

if [[ "${PROJECT_ORACLE_WINDOW_DRY_RUN:-0}" == "1" ]]; then
  echo "DRY RUN: $window_title"
  exit 0
fi

exec "$executable" "$@"
