import importlib.util
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location("package_release", Path(__file__).with_name("package_release.py"))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class PackageTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(dir=Path(__file__).parent)
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        for name in ["package.json.meta", "Editor.meta", "Scripts.meta", "LICENSE", "LICENSE.meta", "README.md", "README.md.meta", "Editor/example.cs", "Scripts/example.cs"]:
            target = self.root / name
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text("fixture", encoding="utf-8")
        self.manifest = dict(name="me.sachino-toto.physbone-modifier", version="1.0.0")
        self.save_manifest()
        self.git("init")
        self.git("add", ".")
        self.git("-c", "user.name=Test", "-c", "user.email=test@example.com", "commit", "-m", "fixture")

    def git(self, *args):
        subprocess.run(["git", *args], cwd=self.root, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)

    def save_manifest(self):
        (self.root / "package.json").write_text(json.dumps(self.manifest), encoding="utf-8")

    def test_root_manifest_and_excludes_untracked(self):
        (self.root / "secret.txt").write_text("local only")
        result = module.package(self.root)
        with zipfile.ZipFile(result["asset"]) as archive:
            self.assertIn("package.json", archive.namelist())
            self.assertNotIn("secret.txt", archive.namelist())
        self.assertEqual(result["tag"], "v1.0.0")
        self.assertEqual(result["prerelease"], "false")

    def test_invalid_semver(self):
        self.manifest["version"] = "1.0.01"
        self.save_manifest()
        with self.assertRaises(ValueError):
            module.package(self.root)

    def test_uncommitted_manifest(self):
        self.manifest["version"] = "1.0.1"
        self.save_manifest()
        with self.assertRaisesRegex(ValueError, "committed HEAD"):
            module.package(self.root)
