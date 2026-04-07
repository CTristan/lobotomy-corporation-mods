#!/usr/bin/env bash
# SPDX-License-Identifier: MIT
#
# Helper script to clean NuGet cache and reinstall local dotnet tool(s)
# Usage: ./scripts/tool-reinstall.sh [tool-name|all]
#   tool-name: Name of the tool (e.g., "ci" for lobotomycorporationmods.ci)
#              If not specified, reinstalls all local tools
#   all:       Reinstall all local tools (ci)

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# List of local tools in this repo
LOCAL_TOOLS="ci"

# Function to get project directory for a tool
get_project_dir() {
    case "$1" in
        ci) echo "CI" ;;
        *) echo "" ;;
    esac
}

# Function to reinstall a single tool
reinstall_tool() {
    local tool_name="$1"
    local project_dir=$(get_project_dir "$tool_name")

    if [ -z "$project_dir" ]; then
        echo -e "${RED}ERROR: Unknown tool '${tool_name}'${NC}"
        return 1
    fi

    local package_id="lobotomycorporationmods.${tool_name}"
    local project_file="${project_dir}/${project_dir}.csproj"
    local nupkg_dir="nupkg"

    # Validate project exists
    if [ ! -f "$project_file" ]; then
        echo -e "${RED}ERROR: Project file not found: $project_file${NC}"
        echo "Are you in the repository root?"
        return 1
    fi

    echo -e "${BLUE}======================================================================${NC}"
    echo -e "${BLUE}Tool Reinstall: ${tool_name}${NC}"
    echo -e "${BLUE}Package: ${package_id}${NC}"
    echo -e "${BLUE}======================================================================${NC}"

    # Step 1: Clean and rebuild
    echo -e "\n${GREEN}[1/5]${NC} Cleaning and rebuilding ${project_dir}..."
    dotnet clean "$project_file"
    dotnet build "$project_file" --configuration Release

    # Step 2: Pack
    echo -e "\n${GREEN}[2/5]${NC} Packing new version..."
    rm -f "${nupkg_dir}/${package_id}".*.nupkg
    dotnet pack "$project_file" --configuration Release --output "./${nupkg_dir}"

    # Step 3: Clear NuGet cache
    echo -e "\n${GREEN}[3/5]${NC} Clearing NuGet cache for ${package_id}..."
    local cache_dir="$HOME/.nuget/packages/${package_id}/1.0.0"
    if [ -d "$cache_dir" ]; then
        rm -rf "$cache_dir"
        echo -e "  ${YELLOW}Removed${NC} $cache_dir"
    else
        echo -e "  ${YELLOW}Cache directory not found (already clean)${NC}"
    fi

    # Step 4: Uninstall tool
    echo -e "\n${GREEN}[4/5]${NC} Uninstalling local tool..."
    if dotnet tool uninstall "$package_id" --local 2>/dev/null; then
        echo -e "  ${GREEN}Uninstalled${NC} $package_id"
    else
        echo -e "  ${YELLOW}Tool not installed (skipping uninstall)${NC}"
    fi

    # Step 5: Reinstall tool
    echo -e "\n${GREEN}[5/5]${NC} Reinstalling local tool..."
    dotnet tool install "$package_id" --local --add-source "./${nupkg_dir}"

    echo -e "\n${GREEN}======================================================================${NC}"
    echo -e "${GREEN}✓ Tool '${tool_name}' reinstalled successfully!${NC}"
    echo -e "${GREEN}======================================================================${NC}\n"
    echo "You can now use: dotnet ${tool_name}"
}

# Determine which tools to reinstall
TOOL_ARG="${1:-}"

if [ -z "$TOOL_ARG" ] || [ "$TOOL_ARG" = "all" ]; then
    # Reinstall all local tools
    echo -e "${BLUE}======================================================================${NC}"
    echo -e "${BLUE}Reinstalling all local tools...${NC}"
    echo -e "${BLUE}======================================================================${NC}"

    for tool in $LOCAL_TOOLS; do
        reinstall_tool "$tool"
    done

    echo -e "${GREEN}======================================================================${NC}"
    echo -e "${GREEN}✓ All local tools reinstalled successfully!${NC}"
    echo -e "${GREEN}======================================================================${NC}\n"
else
    # Reinstall specific tool
    if get_project_dir "$TOOL_ARG" > /dev/null 2>&1; then
        reinstall_tool "$TOOL_ARG"
    else
        echo -e "${RED}ERROR: Unknown tool '${TOOL_ARG}'${NC}"
        echo "Available tools: ${LOCAL_TOOLS}"
        exit 1
    fi
fi
