#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Project Oracle needs the .NET 10 SDK. The dotnet command was not found." >&2
  exit 1
fi

exec dotnet run --project src/ProjectOracle.Console/ProjectOracle.Console.csproj -- "$@"
