#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"
executable="$project_root/Project_Oracle_v0_0_26"

if [[ -x "$executable" && "${PROJECT_ORACLE_FORCE_DOTNET_RUN:-0}" != "1" ]]; then
    exec "$executable" "$@"
fi

if ! command -v dotnet >/dev/null 2>&1; then
    echo "Project Oracle needs the .NET 10 SDK for a development launch. The validated Project_Oracle_v0_0_26 executable is missing." >&2
    exit 1
fi

exec dotnet run --project src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj -- "$@"
