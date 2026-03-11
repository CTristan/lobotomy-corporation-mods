#!/usr/bin/env python3
"""
Script to move ALL package versions from .csproj and Directory.Build.props files 
to Directory.Packages.props. This enables Central Package Management in NuGet.

For SDK-implicit packages (Microsoft.NETFramework.ReferenceAssemblies*), these are
removed entirely from both Directory.Packages.props and the csproj files, as the SDK
handles them implicitly.

Usage:
    python3 scripts/move_packages_to_central.py [--dry-run]
"""

import argparse
import re
import sys
from collections import OrderedDict
from pathlib import Path
from typing import Dict, Set

PROJECT_ROOT = Path(__file__).parent.parent.absolute()

# These packages are handled implicitly by the SDK and should NOT be in central management
SDK_IMPLICIT_PACKAGES = {
    'Microsoft.NETFramework.ReferenceAssemblies',
    'Microsoft.NETFramework.ReferenceAssemblies.net35',
}


def parse_packages_props(packages_props_path: Path) -> Dict[str, str]:
    """Get all package names and versions defined in Directory.Packages.props."""
    existing = OrderedDict()
    try:
        content = packages_props_path.read_text()
        for match in re.finditer(r'<PackageVersion\s+Include="([^"]+)"\s+Version="([^"]+)"', content):
            pkg = match.group(1)
            ver = match.group(2)
            if pkg not in existing:
                existing[pkg] = ver
    except Exception as e:
        print(f"  Warning: Could not parse {packages_props_path}: {e}", file=sys.stderr)
    return existing


def write_packages_props(packages_props_path: Path, packages: Dict[str, str]) -> None:
    """Rewrite Directory.Packages.props with packages (excluding SDK-implicit ones)."""
    lines = [
        '<?xml version="1.0" encoding="utf-8"?>',
        '<Project>',
        '  <PropertyGroup>',
        '    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>',
        '  </PropertyGroup>',
        '  <ItemGroup>',
    ]
    for pkg, ver in sorted(packages.items()):
        if pkg in SDK_IMPLICIT_PACKAGES:
            continue
        lines.append(f'    <PackageVersion Include="{pkg}" Version="{ver}" />')
    lines.extend([
        '  </ItemGroup>',
        '</Project>',
    ])
    packages_props_path.write_text('\n'.join(lines) + '\n')


def parse_csproj_packages(csproj_path: Path) -> Dict[str, str]:
    """Extract PackageReference includes and versions from a .csproj file."""
    packages = {}
    try:
        content = csproj_path.read_text()
        for match in re.finditer(r'<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"', content):
            packages[match.group(1)] = match.group(2)
    except Exception as e:
        print(f"  Warning: Could not parse {csproj_path}: {e}", file=sys.stderr)
    return packages


def parse_directory_build_props(build_props_path: Path) -> Dict[str, str]:
    """Extract PackageReference versions from Directory.Build.props."""
    packages = {}
    try:
        content = build_props_path.read_text()
        for match in re.finditer(r'<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"', content):
            packages[match.group(1)] = match.group(2)
    except Exception as e:
        print(f"  Warning: Could not parse {build_props_path}: {e}", file=sys.stderr)
    return packages


def remove_sdk_implicit_packages_from_csproj(csproj_path: Path) -> int:
    """Remove PackageReference elements for SDK-implicit packages from csproj."""
    modified = 0
    try:
        content = csproj_path.read_text()
        original = content
        
        # Pattern to match PackageReference block (with child elements)
        pattern = r'\s*<PackageReference\s+Include="(' + '|'.join(re.escape(p) for p in SDK_IMPLICIT_PACKAGES) + r')"[^>]*>.*?</PackageReference>\s*\n?'
        
        def remove_block(match):
            nonlocal modified
            modified += 1
            return '\n'
        
        content = re.sub(pattern, remove_block, content, flags=re.DOTALL)
        
        # Also handle self-closing tags
        pattern2 = r'\s*<PackageReference\s+Include="(' + '|'.join(re.escape(p) for p in SDK_IMPLICIT_PACKAGES) + r')"[^>]*/>\s*\n?'
        content = re.sub(pattern2, '\n', content)
        
        if modified > 0:
            csproj_path.write_text(content)
            print(f"  {csproj_path.name}: removed {modified} SDK-implicit PackageReference(s)")
        
    except Exception as e:
        print(f"  Warning: Could not process {csproj_path}: {e}", file=sys.stderr)
    return modified


def remove_versions_from_csproj(csproj_path: Path, central_versions: Set[str]) -> int:
    """Remove Version attribute from remaining PackageReference elements that are in central."""
    modified = 0
    try:
        content = csproj_path.read_text()
        
        def remove_version(match):
            nonlocal modified
            full_match = match.group(0)
            include_match = re.search(r'Include="([^"]+)"', full_match)
            version_match = re.search(r'Version="[^"]+"', full_match)
            
            if include_match and version_match:
                include = include_match.group(1)
                if include in central_versions:
                    modified += 1
                    return re.sub(r'\s+Version="[^"]+"', '', full_match)
            
            return full_match
        
        content = re.sub(r'<PackageReference[^>]+>', remove_version, content)
        
        if modified > 0:
            csproj_path.write_text(content)
            print(f"  {csproj_path.name}: removed {modified} Version(s)")
        
    except Exception as e:
        print(f"  Warning: Could not process {csproj_path}: {e}", file=sys.stderr)
    return modified


def remove_versions_from_props(props_path: Path) -> int:
    """Remove Version attribute from PackageReference elements in a props file."""
    modified = 0
    try:
        content = props_path.read_text()
        
        def remove_version(match):
            nonlocal modified
            full_match = match.group(0)
            if 'Version=' in full_match:
                modified += 1
                return re.sub(r'\s+Version="[^"]+"', '', full_match)
            return full_match
        
        content = re.sub(r'<PackageReference[^>]+>', remove_version, content)
        
        if modified > 0:
            props_path.write_text(content)
            print(f"  {props_path.name}: removed {modified} Version attribute(s)")
    except Exception as e:
        print(f"  Warning: Could not process {props_path}: {e}", file=sys.stderr)
    return modified


def main():
    parser = argparse.ArgumentParser(description='Move package versions to Directory.Packages.props')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be done without making changes')
    args = parser.parse_args()
    
    packages_props = PROJECT_ROOT / 'Directory.Packages.props'
    build_props = PROJECT_ROOT / 'Directory.Build.props'
    
    if not packages_props.exists():
        print(f"Error: {packages_props} not found!")
        sys.exit(1)
    if not build_props.exists():
        print(f"Error: {build_props} not found!")
        sys.exit(1)
    
    print("=== Moving ALL package versions to Directory.Packages.props ===\n")
    print(f"Note: Removing SDK-implicit packages: {SDK_IMPLICIT_PACKAGES}\n")
    
    csproj_files = list(PROJECT_ROOT.rglob('*.csproj'))
    
    # Step 1: Remove SDK-implicit packages from csproj files FIRST
    print("Step 1: Removing SDK-implicit PackageReferences from .csproj files...")
    total_removed = 0
    for csproj in csproj_files:
        count = remove_sdk_implicit_packages_from_csproj(csproj)
        total_removed += count
    print(f"  Total: removed {total_removed} SDK-implicit PackageReference(s)\n")
    
    # Step 2: Get existing central packages
    print("Step 2: Reading existing central packages...")
    central_packages = parse_packages_props(packages_props)
    print(f"  Found {len(central_packages)} packages\n")
    
    # Step 3: Get packages from Directory.Build.props
    print("Step 3: Scanning Directory.Build.props...")
    build_props_packages = parse_directory_build_props(build_props)
    print(f"  Found {len(build_props_packages)} packages\n")
    
    # Step 4: Collect ALL remaining packages from .csproj files
    print("Step 4: Scanning .csproj files...")
    all_packages = {}
    print(f"  Found {len(csproj_files)} .csproj files")
    
    for csproj in csproj_files:
        packages = parse_csproj_packages(csproj)
        for pkg, ver in packages.items():
            if pkg not in central_packages:
                all_packages[pkg] = ver
    
    print(f"  Found {len(all_packages)} unique packages NOT in central management\n")
    
    # Step 5: Rewrite Directory.Packages.props (removing SDK-implicit packages)
    if not args.dry_run:
        print("Step 5: Updating Directory.Packages.props...")
        # Remove SDK-implicit from existing
        for pkg in SDK_IMPLICIT_PACKAGES:
            if pkg in central_packages:
                del central_packages[pkg]
                print(f"  Removed from central: {pkg}")
        write_packages_props(packages_props, central_packages)
        print()
    
    # Step 6: Remove Version from props
    print("Step 6: Removing Version attributes from Directory.Build.props...")
    if build_props_packages:
        remove_versions_from_props(build_props)
    print()
    
    # Step 7: Remove Version from remaining csproj packages
    print("Step 7: Removing Version attributes from .csproj files...")
    central_set = set(central_packages.keys())
    total_versions = 0
    for csproj in csproj_files:
        count = remove_versions_from_csproj(csproj, central_set)
        total_versions += count
    print(f"  Total: removed {total_versions} Version(s)\n")
    
    # Summary
    print("=== Summary ===")
    print(f"  Packages in Directory.Packages.props: {len(central_packages)} (excludes SDK-implicit)")
    print(f"  SDK-implicit PackageReferences removed: {total_removed}")
    print(f"  Version attributes removed: {total_versions}")
    print()
    
    if args.dry_run:
        print("DRY RUN - No files were modified.")
    else:
        print("Done! Run 'dotnet restore' to verify the changes work correctly.")


if __name__ == '__main__':
    main()
