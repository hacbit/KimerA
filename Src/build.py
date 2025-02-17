"""
Analyzers build helper
"""

import os
import shutil
import json

BASE_DIR = os.path.dirname(__file__)

with open(os.path.join(BASE_DIR, "build.json"), encoding="utf8") as f:
    ANALYZER_PROJECTS = json.load(f)["analyzers"]

TARGET_DIR = os.path.join(BASE_DIR, "KimerA", "Assets", "Plugins", "KimerA", "Runtime", "Analyzers")

def get_proj_path(proj_name: str) -> str:
    """
    Get the .csproj file path of the given project name
    """
    return os.path.join(BASE_DIR, proj_name, f"{proj_name}.csproj")

def get_release_dll_path(proj_name: str) -> str:
    """
    Get the release .dll path of the given project name
    """
    return os.path.join(BASE_DIR, proj_name, "bin", "Release", "netstandard2.0", f"{proj_name}.dll")

def build_proj(proj_name: str) -> None:
    """
    Build the given project name
    """
    proj_path = get_proj_path(proj_name)
    os.system(f"dotnet build {proj_path} -c Release")

def main():
    """
    Build all the analyzer projects,
    and copy the .dll files to the target directory
    """
    for proj_name in ANALYZER_PROJECTS:
        proj = get_proj_path(proj_name)
        if not os.path.exists(proj):
            print(f"Project {proj_name} not found")
            continue
        build_proj(proj_name)
        print(f"Project {proj_name} built successfully")
        dll_path = get_release_dll_path(proj_name)
        if not os.path.exists(dll_path):
            print(f"Project {proj_name} .dll not found")
            continue
        if not os.path.exists(TARGET_DIR):
            os.makedirs(TARGET_DIR)
        target = os.path.join(TARGET_DIR, f"{proj_name}.dll")
        shutil.copy(dll_path, target)
        print(f"{proj_name}.dll copied to {target}")

if __name__ == "__main__":
    main()
