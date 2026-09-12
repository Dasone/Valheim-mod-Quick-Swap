# Changelog

## 1.0.1

- **Updated for Valheim 1.0.** The previous release throws as soon as you open your
  inventory, because Valheim 1.0 moved the inventory slot type. This release requires
  Valheim 1.0 or newer, and will not work on older versions.
- Requires BepInExPack Valheim 5.4.2350 or newer.
- Fixed the swap key firing twice per press when two copies of the mod were loaded at once
  — an install from Thunderstore alongside a hot-reload build in `BepInEx/scripts/`, which
  neither BepInEx's nor ScriptEngine's duplicate check catches. Only the newest copy now
  runs; the older one shuts itself down and says so in the log.

## 1.0.0

Initial release.

- Quick swap key toggles between the last two hotbar slots you used.
- Anchor a hotbar slot from the inventory and swap against it from anywhere.
- The anchored slot is badged on the hotbar and in the inventory.
- Every key, the marker colour, and whether consumables count towards your swap history are
  configurable in the `F1` settings window.
