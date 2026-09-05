# Quick Swap

A Valheim BepInEx mod for bouncing between hotbar slots without reaching for the number row.

- **Quick swap** (`Q`) — toggles between the last two hotbar slots you used.
- **Anchored slot** — pin one slot (your pickaxe, your bow, your food). Set it in game by
  hovering a top-row hotbar slot in the open inventory and pressing `Alt + Q`;
  press it on the anchored slot again to clear it. The anchored slot is outlined in colour
  wherever you can see it — on the on-screen hotbar, and in the top row of the open inventory.
- **Anchor swap** (`Alt + Q`) — swaps to the anchored slot and back, from wherever you are:
  anchor 8, select 1, and it swaps 8 and 1; select 2, and it swaps 8 and 2. Leave it unbound
  and the quick swap key takes the anchor on instead, covering both jobs on one key.

## Keys

| Action | Default | What it does |
| --- | --- | --- |
| Quick swap key | `Q` | Toggle between the last two hotbar slots you used. |
| Anchor swap key | `Alt + Q` | Swap to the anchored slot and back. |
| Set anchor bind | `Alt + Q` | With the inventory open, anchor the hotbar slot under the cursor. |

`Alt + Q` doing two jobs is not a clash: setting the anchor only happens while the
inventory is open, swapping only while it is closed. One combination, one anchor concept.

If `Alt + Q` clashes with something else you run, rebind it under `F1` → **Quick Swap**.

Leaving **Anchor swap key** unbound hands the anchor back to the quick swap key, which then
covers both jobs on `Q` alone. There is no setting for that — the binding *is* the setting,
and the load line in the log says which way it resolved:

```
Binds - quick swap: Q | anchor swap: Q + LeftAlt | set anchor: Q + LeftAlt | anchor handled by the anchor swap key
```

## Requirements

- Valheim with BepInEx 5.4.x
- [ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Azus_UnOfficial_ConfigManager/)
  for the in-game `F1` settings window (optional — the config file works without it)
- [ScriptEngine](https://github.com/BepInEx/BepInEx.Debug/releases) for hot reload (development only)

## Configuration

Press `F1` in game and pick **Quick Swap**, or edit
`BepInEx/config/dev.samspel.quickswap.cfg` directly.

| Section | Setting | Default | What it does |
| --- | --- | --- | --- |
| General | Enabled | `true` | Master switch for every hotkey. |
| Keys | Quick swap key | `Q` | Last two slots used, plus the anchor while no anchor swap key is bound. |
| Keys | Anchor swap key | `Alt + Q` | Optional. Binding it takes the anchor off the quick swap key. |
| Keys | Set anchor bind | `Alt + Q` | Held over a hotbar slot with the inventory open, anchors it. |
| Anchor | Anchored slot | `0` | `0` = none. Normally set with the set anchor bind in game. |
| Anchor | Show anchor marker | `true` | Outlines the anchored slot on the hotbar and in the inventory. |
| Anchor | Marker colour | `#FFD24AF2` | Outline colour, `#RRGGBB` or `#RRGGBBAA`. |
| Behaviour | Only remember equipment | `true` | Eating or drinking won't overwrite your swap history. |
| Behaviour | Show HUD messages | `true` | Brief top-left message when the anchor changes. |

`Q` is unbound in vanilla Valheim, which is why it is the default here.

Defaults only apply to settings that are not already in the config file, so a config written
by an earlier version keeps whatever keys it has. Change them under `F1`, or delete
`dev.samspel.quickswap.cfg` with the game closed to start from the stock defaults.

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
- **Which key owns the anchor is derived, not configured.** An explicit switch alongside the
  keybinds could contradict them, and a carried-over config on the losing side of that
  contradiction looks exactly like the anchor being ignored at random. Binding a key for the
  anchor is already an unambiguous statement of intent, so that is the only input.
- **An empty history plus an anchor used to be a dead end.** The first press jumped to the
  anchor with nothing recorded behind it, so the next press had nowhere to go and the key
  looked broken. That is not an exotic state — it is every fresh login, and every hot reload,
  because the history is static. The swap now seeds itself from whatever is equipped.
- **A bare `Q` also fires on `Alt + Q`.** That follows from checking only the keys a
  shortcut names, which is what keeps binds alive while you hold W to run — so the two
  default swap keys share a main key and testing them in a fixed order would let whichever
  came first swallow the other. The one with more modifiers is tried first instead.
- **A shortcut asking for left alt accepts right alt** (likewise shift and control).
  Nobody binding "alt + Q" means one alt key in particular.
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
  AnchorMarker.cs                    the outline on the hotbar and in the inventory
  GameGuards.cs                      when hotkeys are allowed to fire
  Shortcuts.cs                       keybind check that survives held movement keys
  Notifier.cs                        HUD messages
  ConfigurationManagerAttributes.cs  metadata for the F1 window
  Patches/PlayerPatches.cs           records hotbar usage
  Patches/HotkeyBarPatches.cs        repaints the anchor marker on the hotbar
  Patches/InventoryGridPatches.cs    repaints the anchor marker in the inventory
```
