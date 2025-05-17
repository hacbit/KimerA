"""
Analyzers build helper
"""

import os
import shutil
import json

BASE_DIR = os.path.dirname(__file__)

class Project:
    """
    Project class to represent a project
    """
    def __init__(self, name: str, modify: float = 0.0):
        self.name: str = name
        self.modify: float = modify

with open(os.path.join(BASE_DIR, "build.json"), encoding="utf8") as f:
    data = json.load(f)["analyzers"]
    ANALYZER_PROJECTS: list[Project] = [Project(proj["name"], proj["modify"]) for proj in data]

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

def get_newest_modify_time(proj_path: str) -> float:
    """
    Get the newest modify time of the project.
    Skip the bin/ obj/ folders
    """
    newest_time = 0
    for root, _, files in os.walk(proj_path):
        if "bin" in root or "obj" in root:
            continue
        for file in files:
            file_path = os.path.join(root, file)
            if os.path.getmtime(file_path) > newest_time:
                newest_time = os.path.getmtime(file_path)
    return newest_time

def check_should_build(proj: Project) -> bool:
    """
    Check if the project is modified and should be built
    """
    proj_path = get_proj_path(proj.name)
    if not os.path.exists(proj_path):
        return False
    # Check if the project file is modified
    newest_time = get_newest_modify_time(os.path.dirname(proj_path))
    if newest_time > proj.modify:
        proj.modify = newest_time
        return True
    return False

def main():
    """
    Build all the analyzer projects,
    and copy the .dll files to the target directory
    """
    for proj in ANALYZER_PROJECTS:
        proj_path = get_proj_path(proj.name)
        if not os.path.exists(proj_path):
            print(f"==> Project {proj} not found")
            continue
        if not check_should_build(proj):
            print(f"==> Project {proj.name} not modified, skipping build")
            continue
        build_proj(proj.name)
        print(f"==> Project {proj.name} built successfully")
        # Update build.json
        with open(os.path.join(BASE_DIR, "build.json"), "w", encoding="utf8") as f:
            json.dump(
                {"analyzers": [{"name": p.name, "modify": p.modify} for p in ANALYZER_PROJECTS]},
                f,
                indent=4,
                ensure_ascii=False
            )
        dll_path = get_release_dll_path(proj.name)
        if not os.path.exists(dll_path):
            print(f"==> Project {proj.name} .dll not found")
            continue
        if not os.path.exists(TARGET_DIR):
            os.makedirs(TARGET_DIR)
        target = os.path.join(TARGET_DIR, f"{proj.name}.dll")
        shutil.copy(dll_path, target)
        print(f"==> {proj.name}.dll copied to {target}")

if __name__ == "__main__":
    main()
