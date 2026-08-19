#!/usr/bin/env bash
set -u
set -o pipefail

main() {
    local project_root executable applications desktop icon_source icon_directory icon_target icon_metadata
    project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)" || return 1
    executable="$project_root/Project_Oracle_v0_0_25"
    applications="$HOME/.local/share/applications"
    desktop="$applications/project-oracle.desktop"
    icon_source="$project_root/icons/project-oracle.png"
    icon_directory="$HOME/.local/share/icons/hicolor/256x256/apps"
    icon_target="$icon_directory/project-oracle.png"

    if [[ ! -x "$executable" ]]; then
        echo "DESKTOP FAIL: $executable does not exist or is not executable." >&2
        return 1
    fi
    if [[ ! -f "$icon_source" ]]; then
        echo "DESKTOP FAIL: Oracle icon is missing: $icon_source" >&2
        return 1
    fi
    if ! command -v gio >/dev/null 2>&1; then
        echo "DESKTOP FAIL: gio is required to attach the Oracle icon to the Project_Oracle_v0_0_25 executable." >&2
        return 1
    fi

    mkdir -p "$applications" "$icon_directory" || return 1
    cp "$icon_source" "$icon_target" || return 1
    sed "s#PROJECT_ORACLE_EXEC_PLACEHOLDER#${executable//\\/\\\\}#g" "$project_root/desktop/project-oracle.desktop" > "$desktop" || return 1
    chmod +x "$desktop" || return 1

    gio set -t string "$executable" metadata::custom-icon "file://$icon_target" >/dev/null || return 1
    icon_metadata="$(gio info -a metadata::custom-icon "$executable" 2>/dev/null)" || return 1
    if ! grep -Fq "file://$icon_target" <<<"$icon_metadata"; then
        echo "DESKTOP FAIL: Project_Oracle_v0_0_25 executable did not retain the Oracle custom icon metadata." >&2
        return 1
    fi

    if command -v update-desktop-database >/dev/null 2>&1; then
        update-desktop-database "$applications" >/dev/null 2>&1 || true
    fi
    if command -v gtk-update-icon-cache >/dev/null 2>&1; then
        gtk-update-icon-cache -f -t "$HOME/.local/share/icons/hicolor" >/dev/null 2>&1 || true
    fi

    echo "DESKTOP PASS: $desktop"
    echo "ICON PASS: $icon_target"
    echo "EXECUTABLE ICON PASS: $executable"
    echo "EXECUTABLE PASS: $executable"
    return 0
}

main "$@"
