#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
local_bin="$HOME/.local/bin"
applications="$HOME/.local/share/applications"
mkdir -p "$local_bin" "$applications"

launcher="$local_bin/project-oracle"
desktop="$applications/project-oracle.desktop"

cat > "$launcher" <<LAUNCHER
#!/usr/bin/env bash
set -euo pipefail
exec "${project_root}/project-oracle" "\$@"
LAUNCHER
chmod +x "$launcher"

sed "s#PROJECT_ORACLE_EXEC_PLACEHOLDER#${launcher//\\/\\\\}#g" \
  "$project_root/desktop/project-oracle.desktop" > "$desktop"
chmod +x "$desktop"

if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$applications" >/dev/null 2>&1 || true
fi

echo "DESKTOP PASS: Project Oracle launcher installed at $desktop"
echo "EXECUTABLE PASS: $launcher"
