"""
Syncs the "samples" list in Assets/Mirage/package.json with the folders in Assets/Mirage/Samples~

Run by semantic-release (see .releaserc.yml) so new samples show up in the Unity Package Manager
without having to remember to edit package.json by hand.

- Existing entries keep their order, displayName and description
- Entries whose folder no longer exists are removed
- New folders are appended, using the first paragraph of their README as the description
"""

import json
import os
import sys

PACKAGE_ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Mirage")
PACKAGE_JSON = os.path.join(PACKAGE_ROOT, "package.json")
SAMPLES_DIR = "Samples~"

# folders inside Samples~ that are not importable samples
EXCLUDE = {
    # code used by docs, not a sample project
    "Snippets",
}


def read_description(folder):
    for name in ("README.md", "README.txt"):
        path = os.path.join(folder, name)
        if not os.path.isfile(path):
            continue

        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                # skip headings, notes and empty lines, use first line of normal text
                if not line or line.startswith("#") or line.startswith(">"):
                    continue
                return line

    return ""


def main():
    with open(PACKAGE_JSON, encoding="utf-8") as f:
        package = json.load(f)

    samples_root = os.path.join(PACKAGE_ROOT, SAMPLES_DIR)
    folders = sorted(
        name for name in os.listdir(samples_root)
        if os.path.isdir(os.path.join(samples_root, name)) and name not in EXCLUDE
    )

    samples = []
    known_paths = set()
    for sample in package.get("samples", []):
        folder = sample["path"].replace(SAMPLES_DIR + "/", "", 1)
        if folder not in folders:
            print(f"Removing sample '{sample['displayName']}', folder '{sample['path']}' does not exist")
            continue

        samples.append(sample)
        known_paths.add(folder)

    for folder in folders:
        if folder in known_paths:
            continue

        print(f"Adding sample '{folder}'")
        samples.append({
            "displayName": folder,
            "description": read_description(os.path.join(samples_root, folder)),
            "path": f"{SAMPLES_DIR}/{folder}",
        })

    package["samples"] = samples

    with open(PACKAGE_JSON, "w", encoding="utf-8", newline="\n") as f:
        json.dump(package, f, indent=4, ensure_ascii=False)
        f.write("\n")

    return 0


if __name__ == "__main__":
    sys.exit(main())
