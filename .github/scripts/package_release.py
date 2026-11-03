"""Create a VPM ZIP from tracked files only; no Unity project files or local secrets."""
import json
import os
from pathlib import Path
import re
import subprocess
import zipfile


def package(root):
    manifest = json.loads((root / "package.json").read_text(encoding="utf-8-sig"))
    name = manifest["name"]
    if name != "me.sachino-toto.physbone-modifier":
        raise ValueError("Unexpected package name")
    version = manifest["version"]
    numeric = r"(?:0|[1-9][0-9]*)"
    identifier = r"(?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)"
    pattern = rf"{numeric}\.{numeric}\.{numeric}(?:-{identifier}(?:\.{identifier})*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?"
    if not isinstance(version, str) or not re.fullmatch(pattern, version):
        raise ValueError("package.json version must be SemVer")
    output = root / "artifacts" / f"{name}-{version}.zip"
    output.parent.mkdir(exist_ok=True)
    # Explicit inclusion prevents workflows, editor state and unrelated files entering Unity.
    paths = ["package.json", "package.json.meta", "Editor", "Editor.meta",
             "Scripts", "Scripts.meta", "LICENSE", "LICENSE.meta", "README.md", "README.md.meta"]
    subprocess.run(["git", "archive", "--format=zip", f"--output={output}", "HEAD", "--", *paths],
                   cwd=root, check=True)
    with zipfile.ZipFile(output) as archive:
        packaged = json.loads(archive.read("package.json"))
        if packaged != manifest:
            raise ValueError("Working package.json differs from committed HEAD")
        for required in paths:
            if required in ("Editor", "Scripts"):
                if not any(n.startswith(required + "/") and not n.endswith("/") for n in archive.namelist()):
                    raise ValueError(f"Missing package directory: {required}")
            elif required not in archive.namelist():
                raise ValueError(f"Missing package file: {required}")
    result = dict(tag=f"v{version}", asset=str(output),
                  prerelease=str("-" in version.split("+", 1)[0]).lower())
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as stream:
            stream.writelines(f"{key}={value}\n" for key, value in result.items())
    return result


if __name__ == "__main__":
    print(json.dumps(package(Path(__file__).resolve().parents[2])))
