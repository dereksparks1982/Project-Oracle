#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
executable="$project_root/Project_Oracle_v0_0_17"
applications="$HOME/.local/share/applications"
desktop="$applications/project-oracle.desktop"

[[ -x "$executable" ]] || { echo "DESKTOP FAIL: $executable does not exist or is not executable." >&2; exit 1; }
mkdir -p "$applications"
sed "s#PROJECT_ORACLE_EXEC_PLACEHOLDER#${executable//\\/\\\\}#g" "$project_root/desktop/project-oracle.desktop" > "$desktop"
chmod +x "$desktop"
if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$applications" >/dev/null 2>&1 || true
fi

echo "DESKTOP PASS: $desktop"
echo "EXECUTABLE PASS: $executable"
