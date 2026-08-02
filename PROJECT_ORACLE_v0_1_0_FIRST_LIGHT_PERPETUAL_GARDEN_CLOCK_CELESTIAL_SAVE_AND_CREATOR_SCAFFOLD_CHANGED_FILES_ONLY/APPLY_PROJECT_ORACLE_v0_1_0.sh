#!/usr/bin/env bash
set -euo pipefail

archive_path="${1:-}"
target_root="${PROJECT_ORACLE_TARGET:-$HOME/DKLab/Projects/Project Oracle}"
expected_version='0.1.0'
payload_manifest='PROJECT_ORACLE_v0_1_0_PAYLOAD.sha256'
changed_list='PROJECT_ORACLE_CHANGED_FILES_v0_1_0.txt'

fail() {
  echo "APPLY BLOCKED: $*" >&2
  exit 1
}

[[ -n "$archive_path" ]] || fail "Pass the Project Oracle v0.1.0 ZIP path as the first argument."
[[ -f "$archive_path" ]] || fail "The package was not found: $archive_path"
[[ -d "$target_root" ]] || fail "Concept Build 0 was not found at: $target_root"

command -v unzip >/dev/null 2>&1 || fail "The unzip command is required."
command -v sha256sum >/dev/null 2>&1 || fail "The sha256sum command is required."
command -v dotnet >/dev/null 2>&1 || fail "The .NET 10 SDK is required before Project Oracle v0.1.0 can be installed."

sdk_major="$(dotnet --version | cut -d. -f1)"
[[ "$sdk_major" == '10' ]] || fail "Expected .NET SDK 10.x but found $(dotnet --version)."

[[ -f "$target_root/PROJECT_ORACLE_EXPORT_MANIFEST.sha256" ]] || fail "The Concept Build 0 export manifest is missing."
actual_export_manifest_hash="$(sha256sum "$target_root/PROJECT_ORACLE_EXPORT_MANIFEST.sha256" | awk '{print $1}')"
expected_export_manifest_hash='723a111fca76064feb8852c737ee84844f63603ad825b22d09b67ce5aeb9ace2'
[[ "$actual_export_manifest_hash" == "$expected_export_manifest_hash" ]] || fail "The Concept Build 0 export manifest has changed."

if ! (cd "$target_root" && sha256sum -c PROJECT_ORACLE_EXPORT_MANIFEST.sha256 >/dev/null); then
  fail "The Concept Build 0 files do not match the required base."
fi

if [[ -d "$target_root/.git" ]]; then
  [[ -z "$(git -C "$target_root" status --porcelain)" ]] || fail "The Project Oracle Git working tree is not clean."
fi

temporary_root="$(mktemp -d)"
payload_root="$temporary_root/payload"
backup_root="$temporary_root/backup"
new_paths_file="$temporary_root/new_paths.txt"
installed=false

rollback() {
  if [[ "$installed" != true ]]; then
    return
  fi

  echo "ROLLBACK START: restoring Concept Build 0"
  if [[ -f "$new_paths_file" ]]; then
    while IFS= read -r relative_path; do
      [[ -n "$relative_path" ]] || continue
      rm -f -- "$target_root/$relative_path"
    done < "$new_paths_file"
  fi

  if [[ -d "$backup_root" ]]; then
    while IFS= read -r backup_path; do
      relative_path="${backup_path#"$backup_root/"}"
      mkdir -p -- "$(dirname "$target_root/$relative_path")"
      cp -a -- "$backup_path" "$target_root/$relative_path"
    done < <(find "$backup_root" -type f -print)
  fi
  echo "ROLLBACK PASS: Concept Build 0 restored"
}

cleanup() {
  status=$?
  if [[ $status -ne 0 ]]; then
    rollback
  fi
  rm -rf -- "$temporary_root"
  exit "$status"
}
trap cleanup EXIT

mkdir -p "$payload_root" "$backup_root"
unzip -oq "$archive_path" -d "$payload_root"

[[ -f "$payload_root/$payload_manifest" ]] || fail "The package payload manifest is missing."
[[ -f "$payload_root/Project Oracle/$changed_list" ]] || fail "The changed-file inventory is missing."

if ! (cd "$payload_root" && sha256sum -c "$payload_manifest" >/dev/null); then
  fail "The package payload failed its checksum validation."
fi

while IFS= read -r relative_path; do
  [[ -n "$relative_path" ]] || continue
  [[ "$relative_path" != \#* ]] || continue
  [[ "$relative_path" != /* && "$relative_path" != *'..'* ]] || fail "Unsafe payload path: $relative_path"
  source_path="$payload_root/Project Oracle/$relative_path"
  destination_path="$target_root/$relative_path"
  [[ -f "$source_path" ]] || fail "Payload path is missing: $relative_path"
  [[ ! -d "$destination_path" ]] || fail "A directory blocks the payload file: $relative_path"

  if [[ -e "$destination_path" ]]; then
    mkdir -p -- "$(dirname "$backup_root/$relative_path")"
    cp -a -- "$destination_path" "$backup_root/$relative_path"
  else
    printf '%s\n' "$relative_path" >> "$new_paths_file"
  fi

  mkdir -p -- "$(dirname "$destination_path")"
  cp -a -- "$source_path" "$destination_path"
done < "$payload_root/Project Oracle/$changed_list"

installed=true
chmod +x "$target_root/scripts/run.sh" "$target_root/scripts/validate.sh"

echo "PHASE START: installed Project Oracle v${expected_version} candidate"
if ! (cd "$target_root" && ./scripts/validate.sh); then
  fail "Project Oracle v${expected_version} validation failed."
fi

installed=false
echo "PHASE PASS: Project Oracle v${expected_version} installed and validated"
echo "NEXT: inspect the result, then explicitly accept or reject the build."
