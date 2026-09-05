# Quick Swap

A Valheim BepInEx mod for bouncing between hotbar slots without reaching for the number row.

- **Quick swap** — one key toggles between the last two hotbar slots you used. On slot 1,
  press 2, then tap the key to go back to 1, tap again for 2, and so on.
- **Anchored slot** — pin one slot (your pickaxe, your bow, your food). Set it in game by
  hovering a top-row hotbar slot in the open inventory and pressing the set anchor bind;
  press it on the anchored slot again to clear it. A small coloured bar marks the anchored
  slot on the on-screen hotbar.
- **Anchor quick swap** — a key that toggles between the anchored slot and the last *other*
  slot you used. Anchor 2, then work on 3 and 4, and this swaps between 2 and 4. With no
  anchor set it falls back to the plain quick swap, so one key covers both situations.
- **Anchor swap** — the strict version: always jumps to the anchor and always comes back to
  wherever you were, and does nothing at all when no anchor is set.

## Keys

**Nothing is bound out of the box.** Pick what you want under `F1` → **Quick Swap**; until
then the mod loads and does nothing, and says so in the log.

| Action | What it does |
| --- | --- |
| Quick swap key | Toggle the last two slots used. Ignores the anchor. |
| Anchor quick swap key | Toggle the anchor against the last other slot; plain quick swap when no anchor. |
| Anchor swap key | Jump to the anchor and back. Does nothing without an anchor. |
| Set anchor bind | Anchor the hovered top-row slot (inventory open). Middle mouse is a good choice. |

You do not need all four. Binding just **Anchor quick swap key** and **Set anchor bind**
gives you the whole feature on two keys.

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
| General | Enabled | `true` | Master switch for every hotkey. |
| Keys | Quick swap key | unbound | Toggles between the last two slots used. |
| Keys | Anchor quick swap key | unbound | Toggles the anchor against the last other slot. |
| Keys | Anchor swap key | unbound | Jumps to the anchored slot and back. |
| Keys | Set anchor bind | unbound | Held over a hotbar slot with the inventory open, anchors it. |
| Anchor | Anchored slot | `0` | `0` = none. Normally set with the set anchor bind in game. |
| Anchor | Show marker on hotbar | `true` | Draws the coloured bar under the anchored slot. |
| Anchor | Marker colour | `#FFD24AF2` | `#RRGGBB` or `#RRGGBBAA`. |
| Behaviour | Only remember equipment | `true` | Eating or drinking won't overwrite your swap history. |
| Behaviour | Show HUD messages | `true` | Brief top-left message when the anchor changes. |

`Q` is unbound in vanilla Valheim and is a natural choice for one of the swap keys, but any
free key works.

Defaults only apply to settings that are not already in the config file, so a config written
by an earlier version keeps whatever keys it has. Clear them under `F1`, or delete
`samuel.haggren.quickswap.cfg` with the game closed to start from the unbound defaults.

## Building

The project needs the .NET SDK (8.0 or newer) and resolves its references straight out of
your Valheim and BepInEx folders — no NuGet game assemblies, so it always matches the
version you actually run.

```powershell
dotnet build QuickSwap/QuickSwap.csproj
```

If your Valheim install or r2modman profile lives somewhere other than the defaults, copy
`Environment.props.example` to `Environment.props` and edit the paths. That file is
git-ignored, so the project stays portable.

## Hot reload

Every build deploys itself — there is no separate install step and no script to run.

| Command | Goes to | Effect |
| --- | --- | --- |
| `dotnet build QuickSwap/QuickSwap.csproj` | `BepInEx/scripts/` | ScriptEngine reloads it live |
| `... -p:HotReload=false` | `BepInEx/plugins/QuickSwap/` | Normal install, restart Valheim |
| `... -p:Deploy=false` | `bin/` only | Leaves the game profile alone |

The defaults live in `Environment.props`, so you can flip them there instead of passing `-p:`
every time. Each target clears the other location first: two copies of the same BepInPlugin
GUID is a conflict, not a choice.

To rebuild on every save, leave this running:

```powershell
dotnet watch --project QuickSwap/QuickSwap.csproj build
```

Save a source file and the mod rebuilds, redeploys and reloads in the live game a second or
two later. To reload by hand instead, press `F6` in game.

ScriptEngine settings live in `BepInEx/config/com.bepis.bepinex.scriptengine.cfg`:
`LoadOnStart` loads the mod at launch, `EnableFileSystemWatcher` does the automatic reload,
and `ReloadKey` is the manual one.

The plugin unpatches itself in `OnDestroy`, so reloading does not stack duplicate Harmony
patches — but static state (your current and previous slot) resets on each reload, which is
expected. Every build stamps its timestamp into the assembly and logs it on load, so the
`[Quick Swap]` line in `LogOutput.log` tells you exactly which DLL is running.

## How it works

`Player.UseHotbarItem(int index)` is the single funnel every hotbar activation goes through —
number keys, gamepad hotbar, and this mod's own swaps. A Harmony prefix/postfix pair around it
records the slot, but only when the slot really held something (and, by default, only when that
something was equipment). Both swap actions then just call `UseHotbarItem` again, so the game
does all the real work of equipping and the mod never has to model the player's inventory.

Two details are easy to get wrong and worth naming:

- **Keybinds are not checked with `KeyboardShortcut.IsDown`.** That method also requires that
  no other key on the keyboard is held, which is right for a settings-menu chord but wrong for
  a gameplay bind — holding `W` to run would silently kill a bare `Q` shortcut.
  `Shortcuts.Triggered` checks only the keys the shortcut actually names.
- **Nothing is bound by default,** so a silent mod is the expected first-run state rather
  than a fault. The load line in `LogOutput.log` prints every bind and warns when all four
  are empty, which is the difference between "not configured" and "broken".
- **Whether a hotkey may fire is `Player.TakeInput`'s call, not ours.** That is the game's own
  gate on walking, attacking and the vanilla hotbar keys, so chat, the inventory, the map,
  menus, text viewers, death, cutscenes and teleporting are all covered for free — and stay
  covered if the game adds another case.

## Layout

```
QuickSwap/
  QuickSwapPlugin.cs                 plugin entry, Update loop, anchor-on-middle-click
  SwapController.cs                  slot history and the two swap actions
  ModConfig.cs                       every setting
  AnchorMarker.cs                    the coloured bar on the hotbar
  GameGuards.cs                      when hotkeys are allowed to fire
  Shortcuts.cs                       keybind check that survives held movement keys
  Notifier.cs                        HUD messages
  ConfigurationManagerAttributes.cs  metadata for the F1 window
  Patches/PlayerPatches.cs           records hotbar usage
  Patches/HotkeyBarPatches.cs        repaints the anchor marker
```
