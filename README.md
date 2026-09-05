# Quick Swap

A Valheim BepInEx mod for bouncing between hotbar slots without reaching for the number row.

- **Quick swap** — one key toggles between the last two hotbar slots you used. On slot 1,
  press 2, then tap the swap key to go back to 1, tap again for 2, and so on.
- **Anchored slot** — pin one slot (your pickaxe, your bow, your food) and give it its own key.
  That key always jumps to the anchor and always comes back to wherever you were:
  on 3 → anchor → 3; on 4 → anchor → 4.
- **Set the anchor in-game** — open the inventory and middle-click any top-row hotbar slot.
  Middle-click the anchored slot again to clear it. A small coloured bar marks the anchored
  slot on the on-screen hotbar.

## Defaults

| Action | Default key |
| --- | --- |
| Quick swap (last two slots) | `Q` |
| Jump to / from anchored slot | `9` |
| Set or clear the anchor (inventory open, hovering a slot) | Middle mouse button |
| Open the settings window | `F1` (ConfigurationManager) |

All of these are configurable — see **Configuration** below.

## Requirements

- Valheim with BepInEx 5.4.x
- [ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Azus_UnOfficial_ConfigManager/)
  for the in-game `F1` settings window (optional — the config file works without it)
- [ScriptEngine](https://github.com/BepInEx/BepInEx.Debug/releases) for hot reload (development only)

## Configuration

Press `F1` in game and pick **Quick Swap**, or edit
`BepInEx/config/samuel.haggren.quickswap.cfg` directly.

| Section | Setting | Default | What it does |
| --- | --- | --- | --- |
| General | Enabled | `true` | Master switch for both hotkeys. |
| Keys | Quick swap key | `Q` | Toggles between the last two slots used. |
| Keys | Anchor swap key | `9` | Jumps to the anchored slot and back. |
| Keys | Set anchor bind | `Mouse2` | Held over a hotbar slot with the inventory open, anchors it. |
| Anchor | Anchored slot | `0` | `0` = none. Normally set by middle-clicking in game. |
| Anchor | Show marker on hotbar | `true` | Draws the coloured bar under the anchored slot. |
| Anchor | Marker colour | `#FFD24AF2` | `#RRGGBB` or `#RRGGBBAA`. |
| Behaviour | Only remember equipment | `true` | Eating or drinking won't overwrite your swap history. |
| Behaviour | Show HUD messages | `true` | Brief top-left message when the anchor changes. |

`Q` is unbound in vanilla Valheim, but if another mod claims it, rebind here.

## Building

The project needs the .NET SDK (8.0 or newer) and resolves its references straight out of
your Valheim and BepInEx folders — no NuGet game assemblies, so it always matches the
version you actually run.

```powershell
.\build.ps1
```

If your Valheim or r2modman profile lives somewhere other than the defaults, copy
`Directory.Build.user.props.example` to `Directory.Build.user.props` and edit the paths.
That file is git-ignored, so the project stays portable.

## Hot reload

`build.ps1` drops the Debug build into `BepInEx/scripts/`, which BepInEx's **ScriptEngine**
plugin loads and can reload without restarting the game.

```powershell
.\build.ps1 -Watch
```

Leave that running, save a source file, and the mod rebuilds and reloads in the live game a
second or two later. To reload by hand instead, press `F6` in game.

ScriptEngine settings live in `BepInEx/config/com.bepis.bepinex.scriptengine.cfg`:
`LoadOnStart` loads the mod at launch, `EnableFileSystemWatcher` does the automatic reload,
and `ReloadKey` is the manual one.

The plugin unpatches itself in `OnDestroy`, so reloading does not stack duplicate Harmony
patches — but static state (your current and previous slot) resets on each reload, which is
expected.

## Permanent install

```powershell
.\uninstall-dev.ps1   # drop the hot-reload copy first
.\build.ps1 -Release  # installs to BepInEx\plugins\Samuel-QuickSwap
```

Running both copies at once trips BepInEx's duplicate-GUID check, so only ever keep one.

## How it works

`Player.UseHotbarItem(int index)` is the single funnel every hotbar activation goes through —
number keys, gamepad hotbar, and this mod's own swaps. A Harmony prefix/postfix pair around it
records the slot, but only when the slot really held something (and, by default, only when that
something was equipment). Both swap actions then just call `UseHotbarItem` again, so the game
does all the real work of equipping and the mod never has to model the player's inventory.

## Layout

```
src/QuickSwap/
  QuickSwapPlugin.cs                 plugin entry, Update loop, anchor-on-middle-click
  SwapController.cs                  slot history and the two swap actions
  ModConfig.cs                       every setting
  AnchorMarker.cs                    the coloured bar on the hotbar
  GameGuards.cs                      when hotkeys are allowed to fire
  Notifier.cs                        HUD messages
  ConfigurationManagerAttributes.cs  metadata for the F1 window
  Patches/PlayerPatches.cs           records hotbar usage
  Patches/HotkeyBarPatches.cs        repaints the anchor marker
```
