#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

if [[ -x "$project_root/Project_Oracle_v0_0_18" && "${PROJECT_ORACLE_FORCE_DOTNET_RUN:-0}" != "1" ]]; then
  exec "$project_root/Project_Oracle_v0_0_18" "$@"
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Project Oracle needs the .NET 10 SDK for development launch. The validated root executable is also missing." >&2
  exit 1
fi

exec dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj -- "$@"
