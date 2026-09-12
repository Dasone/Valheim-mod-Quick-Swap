# Quick Swap

Swap between two hotbar slots with one key, instead of reaching for the number row.

You are chopping wood with the axe on slot 1 and a greydwarf turns up. Tap **Q** for the
sword on slot 2, deal with it, tap **Q** again, and you are back on the axe. That is the
whole mod.

- **Quick swap** — one key toggles between the last two hotbar slots you used.
- **Anchored slot** — pin one slot you keep coming back to, and it becomes one end of every
  swap. Anchor your pickaxe, then work through slots 3, 4 and 5: the swap key always bounces
  between the pickaxe and whatever you last had out.
- Works with anything on the hotbar — weapons, tools, the hammer, food.

Client-side only. It does not need to be installed on the server, and it does not care
whether anyone else in your world is running it.

## Keys

| Action | Default |
| --- | --- |
| Swap between the last two slots | `Q` |
| Swap to the anchored slot and back | `Alt + Q` |
| Anchor a slot (inventory open, cursor over a top-row slot) | `Alt + Q` |

`Alt + Q` does two jobs, and they never overlap: with the inventory open it sets the anchor,
with it closed it swaps. Press it on the slot that is already anchored to clear the anchor.

Only the top row of your inventory — the eight hotbar slots — can be anchored.

The anchored slot is marked with a small ⇄ badge in its bottom-right corner, on the hotbar
and in your inventory, so you can always see which one it is.

## Settings

Press `F1` for the settings window ([ConfigurationManager] — optional but recommended), or
edit `BepInEx/config/dev.samspel.quickswap.cfg` by hand.

| Setting | Default | What it does |
| --- | --- | --- |
| Enabled | `true` | Master switch for every hotkey. |
| Quick swap key | `Q` | Toggles between the last two slots used. |
| Anchor swap key | `Alt + Q` | Swaps to the anchored slot and back. |
| Set anchor bind | `Alt + Q` | Anchors the hotbar slot under your cursor. |
| Anchored slot | `0` | `0` = none. Normally set in game rather than here. |
| Show anchor marker | `true` | The ⇄ badge on the anchored slot. |
| Marker colour | `#FFD24AF2` | Badge colour, `#RRGGBB` or `#RRGGBBAA`. |
| Only remember equipment | `true` | Eating or drinking will not overwrite your swap history. |
| Show HUD messages | `true` | Brief message when the anchored slot changes. |

Two settings are worth knowing about:

**Only remember equipment** is why eating a meal does not count as "the last slot you used".
Turn it off if you would rather the swap key treat food and mead like anything else.

**Anchor swap key** decides which key owns the anchor. Leave it unbound and the quick swap
key takes the anchor on itself, so `Q` alone does everything. Bind it — as it is by default —
and `Q` goes back to plain last-two-slots swapping while the anchor lives on its own key.

## Installation

Requires **Valheim 1.0 or newer** and BepInExPack Valheim 5.4.2350 or newer. Version 1.0.0
of this mod does not work on Valheim 1.0 — update it rather than rolling back.

**With a mod manager** (recommended) — install through r2modman or Thunderstore Mod Manager
and launch the game from there.

**By hand** — install [BepInEx for Valheim], then drop `QuickSwap.dll` into
`BepInEx/plugins/`.

## Notes

- `Q` is unbound in vanilla Valheim, which is why it is the default. If another mod claims
  it, rebind under `F1`.
- Swapping to an empty slot does nothing, so the swap key will not leave you empty-handed.
- If a swap key does nothing at all, check `BepInEx/LogOutput.log` — Quick Swap prints its
  bindings on every load, including when two of them collide.

## Source

Code and issue tracker: [github.com/Dasone/Valheim-mod-Quick-Swap][source]

[source]: https://github.com/Dasone/Valheim-mod-Quick-Swap
[ConfigurationManager]: https://valheim.thunderstore.io/package/Azumatt/Azus_UnOfficial_ConfigManager/
[BepInEx for Valheim]: https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/
