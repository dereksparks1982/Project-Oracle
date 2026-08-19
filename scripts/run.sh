#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

if [[ -x "$project_root/Project_Oracle_v0_0_25" && "${PROJECT_ORACLE_FORCE_DOTNET_RUN:-0}" != "1" ]]; then
  exec "$project_root/Project_Oracle_v0_0_25" "$@"
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Project Oracle needs the .NET 10 SDK for a development launch. The normal validated versioned desktop executable is missing." >&2
  exit 1
fi

exec dotnet run --project src/ProjectOracle.Desktop/ProjectOracle.Desktop.csproj -- "$@"
