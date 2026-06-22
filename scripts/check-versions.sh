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
# Print a success line (green when stdout is a TTY).
ok()   { printf '%s[ ok ]%s   %s\n'   "$C_OK"   "$C_OFF" "$*"; }
# Print a warning line (yellow when stdout is a TTY).
warn() { printf '%s[warn]%s   %s\n'   "$C_WARN" "$C_OFF" "$*"; }
# Print a failure line (red when stdout is a TTY).
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

# True if $1 is a dotted version of 2-4 numeric components (e.g. 1.2 or 1.2.3.4).
is_well_formed() { [[ "$1" =~ ^[0-9]+(\.[0-9]+){1,3}$ ]]; }

# Normalize a raw version STRING to X.Y.Z, echoed to stdout; nonzero (and echoes
# nothing) when it is missing or malformed. The single validate-then-normalize
# chokepoint: no caller feeds normalize3 unchecked input (which would silently
# fabricate values like 0.0.0 or abc.0.0). Guards both the csproj source of
# truth and the release tag. Callers report the failure.
normalized_or_fail() {
  is_well_formed "$1" || return 1
  normalize3 "$1"
}

# Resolve a csproj's <AssemblyVersion> to a normalized X.Y.Z (nonzero if missing
# or malformed) — the file-reading wrapper over normalized_or_fail.
resolved_version() {
  normalized_or_fail "$(csproj_version_raw "$1")"
}

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
    # shellcheck disable=SC2012  # mod csproj names are controlled; ls is fine
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

# Sync one mod's Info.xml display suffixes to its csproj version; log each change.
write_mod() {
  local id="$1" dir="$2" csproj ver f changed=0
  # shellcheck disable=SC2012  # mod csproj names are controlled; ls is fine
  csproj=$(ls "$dir"/*.csproj | head -1)
  # Refuse to write a bad version into the display names: it would silently
  # become v0.0.0. The source of truth must be valid before we touch Info.xml.
  if ! ver=$(resolved_version "$csproj"); then
    err "$id: csproj <AssemblyVersion> missing or malformed; refusing to write (${csproj#"$ROOT"/})"
    return 1
  fi
  while IFS= read -r f; do
    local before after
    before=$(cat "$f")
    rewrite_info_version "$f" "$ver"
    after=$(cat "$f")
    if [[ "$before" != "$after" ]]; then
      ok "$id: wrote v$ver -> ${f#"$ROOT"/}"
      changed=1
    fi
  done < <(find "$dir/Info" -mindepth 2 -maxdepth 2 -name 'Info.xml' 2>/dev/null | sort)
  [[ "$changed" -eq 0 ]] && ok "$id: already in sync (v$ver)"
  return 0
}

# ---- check -------------------------------------------------------------------

# Echoes nothing; returns the number of ERRORS for this mod (warnings excluded).
check_mod() {
  local id="$1" dir="$2" csproj ver tagver errors=0 f lang disp found_en=0
  # shellcheck disable=SC2012  # mod csproj names are controlled; ls is fine
  csproj=$(ls "$dir"/*.csproj | head -1)

  # Primary: well-formed source of truth.
  if ! ver=$(resolved_version "$csproj"); then
    err "$id: csproj <AssemblyVersion> missing or malformed (${csproj#"$ROOT"/})"
    return 1
  fi

  # Primary: tag/version match at release time. Validate before normalizing so a
  # malformed tag (e.g. a stray "v" prefix) fails loudly instead of being coerced
  # into a bogus value that the comparison then silently mis-judges.
  if [[ -n "${TAG_VERSION:-}" ]]; then
    if ! tagver=$(normalized_or_fail "$TAG_VERSION"); then
      err "$id: tag version '$TAG_VERSION' is malformed (expected X.Y.Z)"
      errors=$((errors + 1))
    elif [[ "$ver" == "$tagver" ]]; then
      ok "$id: tag v$tagver == csproj v$ver"
    else
      err "$id: tag version v$tagver != csproj <AssemblyVersion> v$ver"
      errors=$((errors + 1))
    fi
  fi

  # Secondary cosmetic: per-locale display suffix.
  while IFS= read -r f; do
    lang=$(basename "$(dirname "$f")")
    [[ "$lang" == "en" ]] && found_en=1
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
      err "$id [en]: display v$disp != source of truth v$ver (run: scripts/check-versions.sh --write $id)"
      errors=$((errors + 1))
    else
      warn "$id [$lang]: display v$disp != source of truth v$ver (translator-owned; not auto-fixed)"
    fi
  done < <(find "$dir/Info" -mindepth 2 -maxdepth 2 -name 'Info.xml' 2>/dev/null | sort)

  # A deployable mod must ship an English Info.xml: the launcher shows that display
  # name, and en is the maintainer-owned baseline (other locales only warn). Without
  # this guard, a discovered mod with zero en Info.xml passes the loop silently.
  if [[ "$found_en" -eq 0 ]]; then
    err "$id [en]: no Info/en/Info.xml found (required for the launcher display name)"
    errors=$((errors + 1))
  fi

  return "$errors"
}

# ---- mode dispatch -----------------------------------------------------------

# Run "write" or "check" over one ModId or every mod ("all"); aggregate errors.
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

# Exercise the display-name regex against golden fixtures; return nonzero on fail.
self_test() {
  local tmp pass=0 fail=0 rc=0
  tmp=$(mktemp -d)

  # Assert actual ($2) equals expected ($3), tallying a pass or fail.
  _expect() { # desc, actual, expected
    if [[ "$2" == "$3" ]]; then ok "test: $1"; pass=$((pass + 1));
    else err "test: $1 -- got [$2] expected [$3]"; fail=$((fail + 1)); fi
  }
  # Write an Info.xml fixture whose <name> is $1 to file $2.
  _name() { printf '<info>\n  <name>%s</name>\n</info>\n' "$1" > "$2"; }
  # Read back the <name> text from an Info.xml fixture file.
  _read() { perl -CSD -ne 'if (m{<name>(.*)</name>}) { print $1; exit }' "$1"; }
  # Write a minimal csproj whose <AssemblyVersion> is $1 (omit/empty for none) to $2.
  _csproj() {
    if [[ -n "$1" ]]; then
      printf '<Project><PropertyGroup><AssemblyVersion>%s</AssemblyVersion></PropertyGroup></Project>\n' "$1" > "$2"
    else
      printf '<Project><PropertyGroup></PropertyGroup></Project>\n' > "$2"
    fi
  }
  # Assert that running $2... exits nonzero, tallying a pass or fail.
  _expect_fail() { # desc, cmd...
    if "${@:2}" >/dev/null 2>&1; then err "test: $1 -- unexpectedly succeeded"; fail=$((fail + 1));
    else ok "test: $1"; pass=$((pass + 1)); fi
  }

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

  # resolved_version: valid -> normalized; missing/malformed -> nonzero (never 0.0.0).
  _csproj "1.2" "$tmp/good.csproj"
  _expect "resolved_version normalizes a valid version" "$(resolved_version "$tmp/good.csproj")" "1.2.0"
  _csproj "" "$tmp/missing.csproj"
  _expect_fail "resolved_version rejects a missing version" resolved_version "$tmp/missing.csproj"
  _csproj "1.2.3.4.5" "$tmp/malformed.csproj"
  _expect_fail "resolved_version rejects a malformed version" resolved_version "$tmp/malformed.csproj"

  # normalized_or_fail: the validate-then-normalize chokepoint (also guards TAG_VERSION).
  _expect "normalized_or_fail normalizes a valid string" "$(normalized_or_fail "1.2")" "1.2.0"
  _expect_fail "normalized_or_fail rejects a v-prefixed string" normalized_or_fail "v1.0.0"
  _expect_fail "normalized_or_fail rejects an empty string" normalized_or_fail ""

  # check_mod: a discovered mod with no en Info.xml must hard-fail. The cn display
  # matches its version, so the en guard is the only possible error source.
  mkdir -p "$tmp/mod_noen/Info/cn"
  _csproj "1.2.0" "$tmp/mod_noen/TestNoEn.csproj"
  _name "测试 v1.2.0" "$tmp/mod_noen/Info/cn/Info.xml"
  _expect_fail "check_mod fails when a mod has no en Info.xml" check_mod "TestNoEn" "$tmp/mod_noen"

  # check_mod: the same shape with an en Info.xml present passes (errors == 0).
  mkdir -p "$tmp/mod_en/Info/en"
  _csproj "1.2.0" "$tmp/mod_en/TestEn.csproj"
  _name "Test Mod v1.2.0" "$tmp/mod_en/Info/en/Info.xml"
  if check_mod "TestEn" "$tmp/mod_en" >/dev/null 2>&1; then
    ok "test: check_mod passes when en Info.xml is present"; pass=$((pass + 1))
  else
    err "test: check_mod passes when en Info.xml is present -- unexpectedly failed"; fail=$((fail + 1))
  fi

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

# Print usage to stderr and exit 2.
usage() {
  cat >&2 <<EOF
Usage:
  $0 --write <ModId|all>    rewrite Info.xml display versions from csproj
  $0 --check <ModId|all>    verify versions (set TAG_VERSION to also match a tag)
  $0 --test                 run self-tests
EOF
  exit 2
}

# Parse the mode flag and dispatch to --write, --check, or --test.
main() {
  [[ $# -ge 1 ]] || usage
  case "$1" in
    --write) [[ $# -eq 2 ]] || usage; run_over_target write "$2" ;;
    --check) [[ $# -eq 2 ]] || usage; run_over_target check "$2" ;;
    --test)  [[ $# -eq 1 ]] || usage; self_test ;;
    *) usage ;;
  esac
}

# Run only when executed directly; allow `source`-ing as a library (this lets
# scripts/package-mods.sh reuse discover_mods + the version helpers, so the
# packager and the drift gate always agree on what a mod is).
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
  main "$@"
fi
