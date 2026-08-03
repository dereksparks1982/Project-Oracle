#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Project Oracle needs the .NET 10 SDK. The dotnet command was not found." >&2
  exit 1
fi

if [[ ! -t 0 && "${PROJECT_ORACLE_ALLOW_NONINTERACTIVE:-0}" != "1" && "$*" != *"--once"* ]]; then
  echo "Project Oracle needs an interactive input stream for the live Creator console." >&2
  echo "Use ./scripts/run-window.sh for the separate Garden console window." >&2
  echo "For scripted checks, set PROJECT_ORACLE_ALLOW_NONINTERACTIVE=1 or use --once." >&2
  exit 1
fi

exec dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj -- "$@"
