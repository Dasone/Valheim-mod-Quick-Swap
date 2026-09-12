# Developing Quick Swap

Player-facing documentation is in [README.md](README.md); this covers building the mod.

## Requirements

- .NET SDK 8.0 or newer
- Valheim with BepInEx 5.4.x installed in an r2modman profile

References resolve straight out of your Valheim and BepInEx folders — no NuGet game
assemblies — so the build always matches the version you actually run. `assembly_valheim` is
publicized at build time by `BepInEx.AssemblyPublicizer.MSBuild`, which is what makes
`InventoryGrid.Element`, `HotkeyBar.m_elements` and `Player.TakeInput` reachable.

If your install lives somewhere other than the defaults in `Directory.Build.props`, copy
`Environment.props.example` to `Environment.props` and edit the paths. That file is
git-ignored, so the project stays portable.

## Building

```powershell
dotnet build QuickSwap/QuickSwap.csproj
```

Every build deploys itself. There is no separate install step.

| Command | Goes to | Effect |
| --- | --- | --- |
| `dotnet build QuickSwap/QuickSwap.csproj` | `BepInEx/scripts/` | ScriptEngine reloads it live |
| `... -p:HotReload=false` | `BepInEx/plugins/QuickSwap/` | Normal install, restart Valheim |
| `... -p:Deploy=false` | `bin/` only | Leaves the game profile alone |

Each target clears the other location first: two copies of the same BepInPlugin GUID is a
conflict, not a choice. The defaults live in `Environment.props`, so you can flip them there
instead of passing `-p:` every time.

It only clears `BepInEx/plugins/QuickSwap/`, though — an r2modman install of the published
mod lives in `BepInEx/plugins/<Author>-QuickSwap/` and is left alone, so you can end up
running it alongside your build. The mod now notices and shuts the older copy down (see
below), but the tidier fix is to disable the published one in r2modman while you work.

## Hot reload

The Debug build lands in `BepInEx/scripts/`, which BepInEx's [ScriptEngine] plugin loads and
reloads without restarting the game. To rebuild on every save:

```powershell
dotnet watch --project QuickSwap/QuickSwap.csproj build
```

Save a file and the mod rebuilds, redeploys and reloads a second or two later. `F6` reloads
by hand. ScriptEngine's own settings are in
`BepInEx/config/com.bepis.bepinex.scriptengine.cfg`: `LoadOnStart` loads the mod at launch,
`EnableFileSystemWatcher` does the automatic reload.

`OnDestroy` has to undo everything the plugin added — Harmony patches, marker objects, the
sprite texture — or each reload stacks another copy. Static state (your current and previous
slot) resets on reload, which is expected. Every build stamps its timestamp into the assembly
and logs it on load, so the `[Quick Swap]` line tells you exactly which DLL is running.

## After a Valheim update

Build first. References resolve out of the live game folder, so a renamed or moved game type
is a compile error rather than something to go looking for:

```powershell
dotnet build QuickSwap/QuickSwap.csproj -p:Deploy=false
```

What the compiler cannot check is the three Harmony targets named by string, so confirm those
still exist and still take the same parameter names — `Player.UseHotbarItem(int index)`,
`HotkeyBar.UpdateIcons`, `InventoryGrid.UpdateGui`. Decompiling is the quickest way (see
[the decompile note](#decompiling-the-game) below).

A game-side rename fails late and quietly: a patch on a method that still exists applies
fine, and the mod only throws when the postfix first runs. Valheim 1.0 moved
`InventoryGrid.Element` out to a top-level `InventoryElement` (with `m_pos` becoming
`Position` and `m_go` becoming the component's own `gameObject`), which meant the mod loaded
happily and then threw a `TypeLoadException` the moment anyone opened their inventory.

## Decompiling the game

Valheim's own code is in `assembly_valheim.dll`, not `Assembly-CSharp.dll` — that one holds
only Xbox and PlayFab sample scripts.

```powershell
dotnet tool install -g ilspycmd --version 9.0.0.7889
ilspycmd -o <outdir> -r "<ValheimDir>\valheim_Data\Managed" -t InventoryGrid "<ValheimDir>\valheim_Data\Managed\assembly_valheim.dll"
```

The `-r` reference path is required or type resolution fails, and `ilspycmd` without a pinned
version fails to install on an 8.0 SDK.

## Releasing

```powershell
dotnet build QuickSwap/QuickSwap.csproj -t:Package
```

Produces `dist/QuickSwap-<version>.zip`, ready to upload to Thunderstore. The target builds
Release through a nested MSBuild with deployment forced off, so packaging never swaps the DLL
you are playing with, and it refuses to run unless all three version numbers agree:
`<Version>` in the csproj, `ModVersion` in `QuickSwapPlugin.cs`, and `version_number` in
`Thunderstore/manifest.json`. Bump them together, and add a `CHANGELOG.md` entry.

## Artwork

`art/make_icon.py` (Pillow) draws both the Thunderstore icon and the in-game badge glyph, so
the two stay the same picture:

```powershell
python art/make_icon.py
```

It writes `Thunderstore/icon.png` (256x256, 5px black border) and
`QuickSwap/Assets/anchor-arrows.png`, which is embedded in the DLL so the mod ships as a
single file. The glyph is white on transparent and tinted at runtime, which is how the marker
colour setting works.

## Layout

```
QuickSwap/
  QuickSwapPlugin.cs                 plugin entry, Update loop, anchor-on-cursor
  SwapController.cs                  slot history and the swap actions
  ModConfig.cs                       every setting
  AnchorMarker.cs                    the badge on the hotbar and in the inventory
  MarkerSprite.cs                    loads the embedded swap-arrows glyph
  GameGuards.cs                      when hotkeys are allowed to fire
  Shortcuts.cs                       keybind checks that survive held movement keys
  SingleInstance.cs                  picks one copy when two are loaded at once
  Notifier.cs                        HUD messages
  ConfigurationManagerAttributes.cs  metadata for the F1 window
  Patches/PlayerPatches.cs           records hotbar usage
  Patches/HotkeyBarPatches.cs        repaints the marker on the hotbar
  Patches/InventoryGridPatches.cs    repaints the marker in the inventory
  Assets/anchor-arrows.png           badge glyph, embedded in the DLL
```

## How it works

`Player.UseHotbarItem(int index)` is the single funnel every hotbar activation goes through —
number keys, gamepad hotbar, and this mod's own swaps. A Harmony prefix/postfix pair around it
records the slot, but only when the slot really held something (and, by default, only when
that something was equipment). Both swap actions then call `UseHotbarItem` again, so the game
does all the real work of equipping and the mod never models the player's inventory.

Six things are easy to get wrong here and are worth knowing before changing any of it.

**Keybinds are not checked with `KeyboardShortcut.IsDown`.** That method also requires that no
other key on the keyboard is held, which is right for a settings-menu chord but wrong for a
gameplay bind — holding `W` to run would silently kill a bare `Q` shortcut.
`Shortcuts.Triggered` checks only the keys the shortcut names.

**The cost of that is that a bare `Q` also fires on `Alt + Q`.** The two default swap keys
share a main key, so testing them in a fixed order would let whichever came first swallow the
other. The shortcut with more modifiers is tried first instead.

**Whether a hotkey may fire is `Player.TakeInput`'s call, not ours.** That is the game's own
gate on walking, attacking and the vanilla hotbar keys, so chat, the inventory, the map,
menus, text viewers, death, cutscenes and teleporting are all covered — and stay covered if
the game adds another case.

**An empty history plus an anchor used to be a dead end.** The first press jumped to the
anchor with nothing recorded behind it, so the next press had nowhere to go. That is every
fresh login and every hot reload, because the history is static; the swap now seeds itself
from whatever is equipped.

**Two copies of the mod can be loaded at once, and it does not look like that.** BepInEx
rejects a duplicate GUID inside `plugins/`, and ScriptEngine checks for one too — but it
reads `Chainloader.PluginInfos` before the chainloader has filled it and registers its own
entry a frame later, so an install from Thunderstore and a build in `scripts/` both go live.
That is two `Update` loops and, because ScriptEngine renames the assembly on load, two
independent copies of every static field. Both record every `UseHotbarItem` call including
the other's, so one keypress fires two swaps against a history the other copy already moved:
equip-then-unequip, two equip animations, or the swap stuck on the wrong slot. `SingleInstance`
settles it on load — newest version wins, then newest build stamp — and says so in the log.
Its `Harmony` ID is per instance for the same reason: the loser's `UnpatchSelf` runs at the
end of the frame, after the winner has already patched.

**`Texture2D.LoadImage` is called through reflection.** Unity 6 added `ReadOnlySpan`
overloads, and the compiler has to resolve the whole overload set to pick one — which fails
on `net462`, where neither Valheim's `mscorlib` nor its `netstandard` facade defines
`ReadOnlySpan`. Naming the `byte[]` overload explicitly skips the set and binds at runtime,
where it exists. Expect the same wall on other Unity 6 APIs.

[ScriptEngine]: https://github.com/BepInEx/BepInEx.Debug/releases
