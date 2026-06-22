#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# package-mods.sh — build every mod in Release and produce the release zips.
#
# Reuses mod discovery + version helpers from check-versions.sh (sourced as a
# library), so the packager and the drift gate always agree on what a mod is.
#
# Usage:
#   scripts/package-mods.sh <output-dir>
#
# Produces in <output-dir>:
#   <ModId>.zip      one per mod; the single top-level folder is <ModId>/
#   all-mods.zip     every mod together (each under its own <ModId>/ folder)
#   versions.json    { "ModId": "X.Y.Z", ... } — a recomputable manifest
#
# Each mod's build output is its bin/net35/ directory (OutputPath=bin\, the SDK
# appends the TFM; configuration is NOT in the path, so a clean build is done to
# avoid stale Debug output). That directory is zipped verbatim — it is exactly
# the install payload for <game>/LobotomyCorp_Data/BaseMods/<ModId>/.

set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/check-versions.sh
source "$HERE/check-versions.sh"   # discover_mods, resolved_version (+ helpers)

# Build every discovered mod in Release and emit per-mod zips, all-mods.zip, and
# versions.json into <output-dir> (the sole positional argument).
package_main() {
  local out="${1:-}"
  [[ -n "$out" ]] || { echo "usage: $0 <output-dir>" >&2; exit 2; }
  mkdir -p "$out"; out="$(cd "$out" && pwd)"

  local stage; stage="$(mktemp -d)"
  # shellcheck disable=SC2064
  trap "rm -rf '$stage'" EXIT

  local id dir csproj ver count=0
  local manifest_lines="$stage/.versions.tsv"
  : > "$manifest_lines"

  while IFS=$'\t' read -r id dir; do
    # shellcheck disable=SC2012  # mod csproj names are controlled; ls is fine
    csproj=$(ls "$dir"/*.csproj | head -1)
    # Validate the source of truth before building or writing versions.json, so a
    # missing/malformed <AssemblyVersion> fails loudly instead of shipping v0.0.0.
    if ! ver=$(resolved_version "$csproj"); then
      echo "error: $id has a missing or malformed <AssemblyVersion> (${csproj#"$ROOT"/})" >&2
      exit 1
    fi
    echo ">> building $id (v$ver) -c Release"
    rm -rf "${dir:?}/bin" "${dir:?}/obj"
    dotnet build "$csproj" -c Release --nologo -v minimal
    # The build can succeed without producing the expected net35 output (e.g. a
    # retargeted TFM); fail with a clear message instead of a cryptic cp error.
    if [[ ! -d "$dir/bin/net35" ]]; then
      echo "error: $id build succeeded but produced no bin/net35 output" >&2
      exit 1
    fi
    mkdir -p "$stage/$id"
    cp -R "$dir/bin/net35/." "$stage/$id/"
    ( cd "$stage" && zip -qr "$out/$id.zip" "$id" )
    printf '%s\t%s\n' "$id" "$ver" >> "$manifest_lines"
    count=$((count + 1))
    echo "   packaged $out/$id.zip"
  done < <(discover_mods)

  [[ "$count" -gt 0 ]] || { echo "error: no mods discovered" >&2; exit 1; }

  ( cd "$stage" && zip -qr "$out/all-mods.zip" . -x '.versions.tsv' )
  echo "   packaged $out/all-mods.zip ($count mods)"

  # versions.json — recomputable manifest, built from the same data.
  jq -Rn '[inputs | split("\t") | {key: .[0], value: .[1]}] | from_entries' \
    < "$manifest_lines" > "$out/versions.json"
  echo "   wrote $out/versions.json"

  echo "done: $count mods -> $out"
}

package_main "$@"
