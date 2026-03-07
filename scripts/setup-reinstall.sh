#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# Helper script to clean NuGet cache and reinstall local dotnet tool
# Usage: ./scripts/setup-reinstall.sh [tool-name]
#   tool-name: Name of the tool (without package ID, e.g., "setup" for lobotomycorporationmods.setup)
#              If not specified, defaults to "setup"

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default tool name (setup -> lobotomycorporationmods.setup)
TOOL_NAME="${1:-setup}"
PACKAGE_ID="lobotomycorporationmods.${TOOL_NAME}"
PROJECT_DIR="SetupExternal"
PROJECT_FILE="${PROJECT_DIR}/${PROJECT_DIR}.csproj"
NUPKG_DIR="nupkg"

# Validate project exists
if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}ERROR: Project file not found: $PROJECT_FILE${NC}"
    echo "Are you in the repository root?"
    exit 1
fi

echo -e "${BLUE}======================================================================${NC}"
echo -e "${BLUE}Tool Reinstall: ${TOOL_NAME}${NC}"
echo -e "${BLUE}Package: ${PACKAGE_ID}${NC}"
echo -e "${BLUE}======================================================================${NC}"

# Step 1: Clean and rebuild
echo -e "\n${GREEN}[1/5]${NC} Cleaning and rebuilding ${PROJECT_DIR}..."
dotnet clean "$PROJECT_FILE"
dotnet build "$PROJECT_FILE" --configuration Release

# Step 2: Pack
echo -e "\n${GREEN}[2/5]${NC} Packing new version..."
rm -f "${NUPKG_DIR}/${PACKAGE_ID}".*.nupkg
dotnet pack "$PROJECT_FILE" --configuration Release --output "./${NUPKG_DIR}"

# Step 3: Clear NuGet cache
echo -e "\n${GREEN}[3/5]${NC} Clearing NuGet cache for ${PACKAGE_ID}..."
CACHE_DIR="$HOME/.nuget/packages/${PACKAGE_ID}/1.0.0"
if [ -d "$CACHE_DIR" ]; then
    rm -rf "$CACHE_DIR"
    echo -e "  ${YELLOW}Removed${NC} $CACHE_DIR"
else
    echo -e "  ${YELLOW}Cache directory not found (already clean)${NC}"
fi

# Step 4: Uninstall tool
echo -e "\n${GREEN}[4/5]${NC} Uninstalling local tool..."
if dotnet tool uninstall "$PACKAGE_ID" --local 2>/dev/null; then
    echo -e "  ${GREEN}Uninstalled${NC} $PACKAGE_ID"
else
    echo -e "  ${YELLOW}Tool not installed (skipping uninstall)${NC}"
fi

# Step 5: Reinstall tool
echo -e "\n${GREEN}[5/5]${NC} Reinstalling local tool..."
dotnet tool install "$PACKAGE_ID" --local --add-source "./${NUPKG_DIR}"

echo -e "\n${GREEN}======================================================================${NC}"
echo -e "${GREEN}✓ Tool '${TOOL_NAME}' reinstalled successfully!${NC}"
echo -e "${GREEN}======================================================================${NC}\n"
echo "You can now use: dotnet ${TOOL_NAME}"
