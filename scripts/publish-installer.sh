#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# Builds and publishes the Harmony 2 for LMM installer for distribution.
#
# Usage:
#   ./scripts/publish-installer.sh              # Publish all platforms
#   ./scripts/publish-installer.sh win-x64      # Publish specific platform
#   ./scripts/publish-installer.sh --list       # List available platforms
#
# Prerequisites:
#   - BepInEx 5 files must be in Harmony2ForLmm/Resources/bepinex/
#   - .NET SDK must be installed

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

INSTALLER_PROJECT="$REPO_ROOT/Harmony2ForLmm/Harmony2ForLmm.csproj"
RETARGET_PROJECT="$REPO_ROOT/RetargetHarmony/RetargetHarmony.csproj"
RESOURCES_DIR="$REPO_ROOT/Harmony2ForLmm/Resources"
PUBLISH_DIR="$REPO_ROOT/Harmony2ForLmm/bin/publish"

ALL_PLATFORMS="win-x64 win-x86 linux-x64 osx-x64 osx-arm64"

info() { echo -e "${BLUE}[INFO]${NC} $1"; }
success() { echo -e "${GREEN}[OK]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; }

is_valid_platform() {
    local target="$1"
    for p in $ALL_PLATFORMS; do
        if [ "$p" = "$target" ]; then
            return 0
        fi
    done
    return 1
}

show_help() {
    echo "Usage: $(basename "$0") [PLATFORM...|--list|--help]"
    echo ""
    echo "Platforms:"
    for platform in $ALL_PLATFORMS; do
        echo "  $platform (self-contained, single file, trimmed)"
    done
    echo ""
    echo "If no platform is specified, all platforms are published."
}

validate_prerequisites() {
    if [ ! -f "$RESOURCES_DIR/bepinex/winhttp.dll" ]; then
        error "BepInEx distribution files not found in $RESOURCES_DIR/bepinex/"
        error "Download BepInEx 5.4.23.5 (win_x64) and extract to that directory."
        error "See $RESOURCES_DIR/bepinex/README.md for details."
        exit 1
    fi

    if [ ! -f "$REPO_ROOT/RetargetHarmony/lib/0Harmony109.dll" ]; then
        error "0Harmony109.dll not found in RetargetHarmony/lib/"
        exit 1
    fi
}

build_retarget_harmony() {
    info "Building RetargetHarmony..."
    dotnet build "$RETARGET_PROJECT" --verbosity quiet

    local dll="$REPO_ROOT/RetargetHarmony/bin/net35/RetargetHarmony.dll"
    if [ ! -f "$dll" ]; then
        error "RetargetHarmony.dll not found at expected path: $dll"
        exit 1
    fi

    success "RetargetHarmony.dll built"
}

publish_platform() {
    local platform="$1"

    info "Publishing for $platform..."

    dotnet publish "$INSTALLER_PROJECT" \
        -c Release \
        -r "$platform" \
        -o "$PUBLISH_DIR/$platform" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        --verbosity quiet

    success "Published $platform -> $PUBLISH_DIR/$platform/"
}

summarize() {
    echo ""
    echo -e "${GREEN}======================================================================${NC}"
    echo -e "${GREEN}  Publish complete${NC}"
    echo -e "${GREEN}======================================================================${NC}"
    echo ""

    for platform in "$@"; do
        local dir="$PUBLISH_DIR/$platform"
        if [ -d "$dir" ]; then
            local size
            size=$(du -sh "$dir" | cut -f1)
            echo -e "  $platform: ${BLUE}$dir${NC} ($size)"
        fi
    done
    echo ""
}

# --- Main ---

if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    show_help
    exit 0
fi

if [ "${1:-}" = "--list" ]; then
    for platform in $ALL_PLATFORMS; do
        echo "$platform"
    done
    exit 0
fi

validate_prerequisites

# Determine which platforms to publish
if [ $# -gt 0 ]; then
    for platform in "$@"; do
        if ! is_valid_platform "$platform"; then
            error "Unknown platform: $platform"
            show_help
            exit 1
        fi
    done
    PLATFORMS_TO_PUBLISH=("$@")
else
    # shellcheck disable=SC2206
    PLATFORMS_TO_PUBLISH=($ALL_PLATFORMS)
fi

# Clean previous publish output
if [ -d "$PUBLISH_DIR" ]; then
    info "Cleaning previous publish output..."
    rm -rf "$PUBLISH_DIR"
fi

# Build RetargetHarmony and stage its DLL
build_retarget_harmony

# Publish each platform
for platform in "${PLATFORMS_TO_PUBLISH[@]}"; do
    publish_platform "$platform"
done

summarize "${PLATFORMS_TO_PUBLISH[@]}"
