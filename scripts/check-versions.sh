#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# check-versions.sh — single source of truth + drift gate for mod versions.
#
# The csproj <AssemblyVersion> is the SOURCE OF TRUTH for every mod's version.
# Each mod also carries a cosmetic copy of the version in the display name shown
# by the in-game mod launcher (LMM/Basemod): Info/{lang}/Info.xml <name> ends
# with a "v<X.Y.Z>" suffix, e.g. "Gift Alert Icon v1.0.2" (en, with a space) or
# "饰品警告图标v1.0.2" (cn, no space). This script keeps those display suffixes
# in sync with the source of truth and verifies them.
#
# Modes:
#   --write <ModId|all>   Rewrite the trailing v<X.Y.Z> in every Info/*/Info.xml
#                         <name> from the csproj <AssemblyVersion>, preserving
#                         each locale's human title and separator. Sole writer.
#   --check <ModId|all>   Verify versions.
#                           Primary  : csproj <AssemblyVersion> is well-formed,
#                                      and == $TAG_VERSION when that env var is
#                                      set (release-time tag/version match).
#                           Secondary: the English (en) display suffix matches
#                                      the source of truth -> HARD FAIL.
#                                      Other locales (e.g. cn) only WARN, because
#                                      they are translator-owned text.
#                         Exits nonzero only on a primary or English mismatch.
#   --test                Run self-tests against golden fixtures (guards the
#                         display-name regex, the one fragile spot).
#
# Mods are discovered dynamically: any LobotomyCorporationMods.* directory that
# has both a .csproj and Info/GlobalInfo.xml is a deployable mod (the ModId is
# that GlobalInfo's <ID>). Nothing hardcodes the mod list or count, so mods can
# be added or removed without editing this script.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# ---- styling -----------------------------------------------------------------
if [[ -t 1 ]]; then
  C_OK=$'\033[32m'; C_WARN=$'\033[33m'; C_ERR=$'\033[31m'; C_OFF=$'\033[0m'
else
  C_OK=''; C_WARN=''; C_ERR=''; C_OFF=''
fi
ok()   { printf '%s[ ok ]%s   %s\n'   "$C_OK"   "$C_OFF" "$*"; }
warn() { printf '%s[warn]%s   %s\n'   "$C_WARN" "$C_OFF" "$*"; }
err()  { printf '%s[FAIL]%s   %s\n'   "$C_ERR"  "$C_OFF" "$*"; }

# ---- version helpers ---------------------------------------------------------

# Extract the raw <AssemblyVersion> text from a csproj (first occurrence).
csproj_version_raw() {
  perl -ne 'if (m{<AssemblyVersion>\s*([0-9][0-9.]*)\s*</AssemblyVersion>}) { print $1; exit }' "$1"
}

# Normalize a dotted version to exactly three components (X.Y.Z), padding 0s.
normalize3() {
  local v="$1" a b c IFS='.'
  # shellcheck disable=SC2206
  local parts=($v)
  a="${parts[0]:-0}"; b="${parts[1]:-0}"; c="${parts[2]:-0}"
  printf '%s.%s.%s' "$a" "$b" "$c"
}

is_well_formed() { [[ "$1" =~ ^[0-9]+(\.[0-9]+){1,3}$ ]]; }

# Extract the trailing display version (the LAST "v<X.Y.Z>" before </name>).
info_display_version() {
  perl -CSD -ne 'if (m{<name>.*v([0-9]+\.[0-9]+\.[0-9]+)</name>}) { print $1; exit }' "$1"
}

# Rewrite the trailing "v<X.Y.Z>" in a single Info.xml <name> to $2.
# No-op if the name has no such suffix (cannot infer a title/separator).
rewrite_info_version() {
  local file="$1" ver="$2"
  VER="$ver" perl -CSD -i -pe \
    's{(<name>.*)v[0-9]+\.[0-9]+\.[0-9]+(</name>)}{${1}v$ENV{VER}${2}}' "$file"
}

# ---- mod discovery -----------------------------------------------------------

# Print "ModId<TAB>dir" for every deployable mod, sorted by ModId.
discover_mods() {
  local dir csproj global id
  for dir in "$ROOT"/LobotomyCorporationMods.*/; do
    dir="${dir%/}"
    [[ -d "$dir" ]] || continue
    global="$dir/Info/GlobalInfo.xml"
    [[ -f "$global" ]] || continue
    # shellcheck disable=SC2012
    csproj=$(ls "$dir"/*.csproj 2>/dev/null | head -1 || true)
    [[ -n "$csproj" ]] || continue
    id=$(perl -ne 'if (m{<ID>\s*([^<]+?)\s*</ID>}) { print $1; exit }' "$global")
    [[ -n "$id" ]] || continue
    printf '%s\t%s\n' "$id" "$dir"
  done | sort
}

# Resolve a single ModId to its directory (exits nonzero if unknown).
mod_dir_for() {
  local want="$1" id dir
  while IFS=$'\t' read -r id dir; do
    [[ "$id" == "$want" ]] && { printf '%s' "$dir"; return 0; }
  done < <(discover_mods)
  return 1
}

# ---- write -------------------------------------------------------------------

write_mod() {
  local id="$1" dir="$2" csproj raw ver f changed=0
  # shellcheck disable=SC2012
  csproj=$(ls "$dir"/*.csproj | head -1)
  raw=$(csproj_version_raw "$csproj")
  ver=$(normalize3 "$raw")
  while IFS= read -r f; do
    local before after
    before=$(cat "$f")
    rewrite_info_version "$f" "$ver"
    after=$(cat "$f")
    if [[ "$before" != "$after" ]]; then
      ok "$id: wrote v$ver -> ${f#"$ROOT"/}"
      changed=1
    fi
  done < <(find "$dir/Info" -mindepth 2 -name 'Info.xml' 2>/dev/null | sort)
  [[ "$changed" -eq 0 ]] && ok "$id: already in sync (v$ver)"
  return 0
}

# ---- check -------------------------------------------------------------------
# Echoes nothing; returns the number of ERRORS for this mod (warnings excluded).

check_mod() {
  local id="$1" dir="$2" csproj raw ver errors=0 f lang disp
  # shellcheck disable=SC2012
  csproj=$(ls "$dir"/*.csproj | head -1)
  raw=$(csproj_version_raw "$csproj")

  # Primary: well-formed source of truth.
  if [[ -z "$raw" ]] || ! is_well_formed "$raw"; then
    err "$id: csproj <AssemblyVersion> missing or malformed: '${raw:-<none>}'"
    return 1
  fi
  ver=$(normalize3 "$raw")

  # Primary: tag/version match at release time.
  if [[ -n "${TAG_VERSION:-}" ]]; then
    local tagver; tagver=$(normalize3 "$TAG_VERSION")
    if [[ "$ver" != "$tagver" ]]; then
      err "$id: tag version v$tagver != csproj <AssemblyVersion> v$ver"
      errors=$((errors + 1))
    else
      ok "$id: tag v$tagver == csproj v$ver"
    fi
  fi

  # Secondary cosmetic: per-locale display suffix.
  while IFS= read -r f; do
    lang=$(basename "$(dirname "$f")")
    disp=$(info_display_version "$f")
    if [[ -z "$disp" ]]; then
      if [[ "$lang" == "en" ]]; then
        err "$id [en]: no v<X.Y.Z> suffix found in <name> (expected v$ver)"
        errors=$((errors + 1))
      else
        warn "$id [$lang]: no v<X.Y.Z> suffix found in <name> (expected v$ver)"
      fi
      continue
    fi
    if [[ "$disp" == "$ver" ]]; then
      ok "$id [$lang]: display v$disp matches"
    elif [[ "$lang" == "en" ]]; then
      err "$id [en]: display v$disp != source of truth v$ver (run: $0 --write $id)"
      errors=$((errors + 1))
    else
      warn "$id [$lang]: display v$disp != source of truth v$ver (translator-owned; not auto-fixed)"
    fi
  done < <(find "$dir/Info" -mindepth 2 -name 'Info.xml' 2>/dev/null | sort)

  return "$errors"
}

# ---- mode dispatch -----------------------------------------------------------

run_over_target() {
  local action="$1" target="$2" id dir total_errors=0 count=0
  if [[ "$target" == "all" ]]; then
    while IFS=$'\t' read -r id dir; do
      count=$((count + 1))
      if [[ "$action" == "write" ]]; then
        write_mod "$id" "$dir"
      else
        set +e; check_mod "$id" "$dir"; total_errors=$((total_errors + $?)); set -e
      fi
    done < <(discover_mods)
    [[ "$count" -eq 0 ]] && { err "no mods discovered under $ROOT"; return 1; }
  else
    dir=$(mod_dir_for "$target") || { err "unknown mod '$target' (not a discovered mod)"; return 1; }
    if [[ "$action" == "write" ]]; then
      write_mod "$target" "$dir"
    else
      set +e; check_mod "$target" "$dir"; total_errors=$?; set -e
    fi
  fi

  if [[ "$action" == "check" ]]; then
    echo
    if [[ "$total_errors" -gt 0 ]]; then
      err "version check failed with $total_errors error(s)"
      return 1
    fi
    ok "version check passed"
  fi
  return 0
}

# ---- self-tests --------------------------------------------------------------

self_test() {
  local tmp pass=0 fail=0 rc=0
  tmp=$(mktemp -d)

  _expect() { # desc, actual, expected
    if [[ "$2" == "$3" ]]; then ok "test: $1"; pass=$((pass + 1));
    else err "test: $1 -- got [$2] expected [$3]"; fail=$((fail + 1)); fi
  }
  _name() { printf '<info>\n  <name>%s</name>\n</info>\n' "$1" > "$2"; }
  _read() { perl -CSD -ne 'if (m{<name>(.*)</name>}) { print $1; exit }' "$1"; }

  # en with a space separator
  _name "Gift Alert Icon v1.0.2" "$tmp/en.xml"
  rewrite_info_version "$tmp/en.xml" "9.9.9"
  _expect "en (space) rewrite" "$(_read "$tmp/en.xml")" "Gift Alert Icon v9.9.9"

  # cn with no separator (UTF-8)
  _name "饰品警告图标v1.0.1" "$tmp/cn.xml"
  rewrite_info_version "$tmp/cn.xml" "1.0.2"
  _expect "cn (no space, UTF-8) rewrite" "$(_read "$tmp/cn.xml")" "饰品警告图标v1.0.2"

  # title that contains the letter v and stray digits -> must pick the LAST v<semver>
  _name "Mod version 2 Deluxe v1.2.3" "$tmp/tricky.xml"
  rewrite_info_version "$tmp/tricky.xml" "9.9.9"
  _expect "tricky title rewrite" "$(_read "$tmp/tricky.xml")" "Mod version 2 Deluxe v9.9.9"

  # multi-digit major (the WarnWhenDie abnormality-count convention)
  _name "Warn When Agent Will Die From Working v15.0.0" "$tmp/multi.xml"
  rewrite_info_version "$tmp/multi.xml" "16.0.0"
  _expect "multi-digit major rewrite" "$(_read "$tmp/multi.xml")" "Warn When Agent Will Die From Working v16.0.0"

  # no version suffix -> rewrite is a no-op
  _name "No Version Here" "$tmp/none.xml"
  rewrite_info_version "$tmp/none.xml" "9.9.9"
  _expect "no-suffix is left unchanged" "$(_read "$tmp/none.xml")" "No Version Here"

  # display extraction
  _name "Gift Alert Icon v1.0.2" "$tmp/read.xml"
  _expect "display extraction" "$(info_display_version "$tmp/read.xml")" "1.0.2"

  # normalize3
  _expect "normalize 2-part" "$(normalize3 "1.2")" "1.2.0"
  _expect "normalize 4-part" "$(normalize3 "15.0.0.0")" "15.0.0"

  echo
  if [[ "$fail" -gt 0 ]]; then
    err "self-tests failed: $fail failed, $pass passed"; rc=1
  else
    ok "self-tests passed: $pass passed"
  fi
  rm -rf "$tmp"
  return "$rc"
}

# ---- entrypoint --------------------------------------------------------------

usage() {
  cat >&2 <<EOF
Usage:
  $0 --write <ModId|all>    rewrite Info.xml display versions from csproj
  $0 --check <ModId|all>    verify versions (set TAG_VERSION to also match a tag)
  $0 --test                 run self-tests
EOF
  exit 2
}

main() {
  [[ $# -ge 1 ]] || usage
  case "$1" in
    --write) [[ $# -eq 2 ]] || usage; run_over_target write "$2" ;;
    --check) [[ $# -eq 2 ]] || usage; run_over_target check "$2" ;;
    --test)  [[ $# -eq 1 ]] || usage; self_test ;;
    *) usage ;;
  esac
}

main "$@"
