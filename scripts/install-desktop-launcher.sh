#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
executable="$project_root/Project_Oracle_v0_0_24"
applications="$HOME/.local/share/applications"
desktop="$applications/project-oracle.desktop"
icon_source="$project_root/icons/project-oracle.png"
icon_directory="$HOME/.local/share/icons/hicolor/256x256/apps"
icon_target="$icon_directory/project-oracle.png"

[[ -x "$executable" ]] || { echo "DESKTOP FAIL: $executable does not exist or is not executable." >&2; exit 1; }
[[ -f "$icon_source" ]] || { echo "DESKTOP FAIL: Oracle icon is missing: $icon_source" >&2; exit 1; }
mkdir -p "$applications" "$icon_directory"
cp "$icon_source" "$icon_target"
sed "s#PROJECT_ORACLE_EXEC_PLACEHOLDER#${executable//\\/\\\\}#g" "$project_root/desktop/project-oracle.desktop" > "$desktop"
chmod +x "$desktop"

if command -v gio >/dev/null 2>&1; then
    gio set "$executable" metadata::custom-icon "file://$icon_source" >/dev/null 2>&1 || true
fi
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$applications" >/dev/null 2>&1 || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -f -t "$HOME/.local/share/icons/hicolor" >/dev/null 2>&1 || true
fi

echo "DESKTOP PASS: $desktop"
echo "ICON PASS: $icon_target"
echo "EXECUTABLE PASS: $executable"
