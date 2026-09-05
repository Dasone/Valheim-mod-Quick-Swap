using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace QuickSwap
{
    /// <summary>
    /// Quick Swap — toggle between the last two hotbar slots you used, plus an
    /// anchored slot you can bounce off from wherever you happen to be.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInProcess("valheim.exe")]
    public class QuickSwapPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "dev.samspel.quickswap";
        public const string ModName = "Quick Swap";
        public const string ModVersion = "1.0.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;
        private Player _lastLocalPlayer;

        private void Awake()
        {
            Log = Logger;

            ModConfig.Init(Config);

            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll(typeof(QuickSwapPlugin).Assembly);

            Log.LogInfo(ModName + " " + ModVersion + " loaded (" + BuildInfo.Configuration + " build, " + BuildInfo.BuildTime + ").");
            LogBindings();
        }

        /// <summary>
        /// Runs on hot reload as well as on shutdown, so everything this plugin added
        /// to the game has to come back off here — otherwise a reload stacks a second
        /// set of Harmony patches on top of the first.
        /// </summary>
        private void OnDestroy()
        {
            AnchorMarker.RemoveAll();
            MarkerSprite.Unload();
            _harmony?.UnpatchSelf();

            // Deliberately no Config.Save() here. SaveOnConfigSet is on by default, so every
            // change is already on disk — and saving during a hot reload would write this
            // instance's now-stale values back over whatever the file has since become.

            Log.LogInfo(ModName + " unloaded.");
        }

        private void Update()
        {
            Player player = Player.m_localPlayer;

            if (player != _lastLocalPlayer)
            {
                // New character, death respawn, or logout: start the history fresh.
                _lastLocalPlayer = player;
                SwapController.Reset();
            }

            if (player == null || !ModConfig.Enabled.Value)
            {
                return;
            }

            if (InventoryGui.IsVisible())
            {
                if (ModConfig.SetAnchorBind.Value.Triggered())
                {
                    TrySetAnchorUnderCursor();
                }

                return;
            }

            if (!GameGuards.AcceptsGameplayInput(player))
            {
                return;
            }

            HandleSwapKeys();
        }

        /// <summary>
        /// Fires whichever swap key was pressed, preferring the more specific combination.
        /// </summary>
        /// <remarks>
        /// The default binds are Q and alt+Q, which share a main key, so alt+Q also
        /// satisfies the bare Q. Testing in a fixed order would let whichever came first
        /// swallow the other; the modifier count decides instead. A tie means both are
        /// bound to the same combination, which <see cref="ModConfig.HasDedicatedAnchorKey"/>
        /// already treats as "quick swap keeps the anchor", so quick swap wins it.
        /// </remarks>
        private static void HandleSwapKeys()
        {
            KeyboardShortcut quick = ModConfig.QuickSwapKey.Value;
            KeyboardShortcut anchor = ModConfig.AnchorSwapKey.Value;

            bool quickFired = quick.Triggered();
            bool anchorFired = anchor.Triggered();

            if (anchorFired && (!quickFired || anchor.ModifierCount() > quick.ModifierCount()))
            {
                SwapController.AnchorSwap();
            }
            else if (quickFired)
            {
                SwapController.QuickSwap();
            }
        }

        /// <summary>
        /// States the effective binds on load. Two of them can be cleared or pointed at the
        /// same key, and a mod that loads fine and then does nothing looks broken rather
        /// than merely unconfigured — so the log has to be able to tell those apart.
        /// </summary>
        private static void LogBindings()
        {
            Log.LogInfo("Binds - quick swap: " + Describe(ModConfig.QuickSwapKey.Value)
                        + " | anchor swap: " + Describe(ModConfig.AnchorSwapKey.Value)
                        + " | set anchor: " + Describe(ModConfig.SetAnchorBind.Value)
                        + " | anchor handled by the "
                        + (ModConfig.HasDedicatedAnchorKey() ? "anchor swap" : "quick swap") + " key");

            if (ModConfig.AnchorSwapKey.Value.IsBound()
                && ModConfig.AnchorSwapKey.Value.Equals(ModConfig.QuickSwapKey.Value))
            {
                Log.LogWarning("Quick swap and anchor swap are bound to the same key, so only quick swap fires. "
                               + "Give the anchor its own key, or leave it unbound and quick swap will handle it.");
            }

            if (!ModConfig.QuickSwapKey.Value.IsBound() && !ModConfig.AnchorSwapKey.Value.IsBound()
                && !ModConfig.SetAnchorBind.Value.IsBound())
            {
                Log.LogWarning("No keys are bound, so nothing will happen yet. Bind them under F1 -> Quick Swap.");
            }
        }

        private static string Describe(KeyboardShortcut shortcut)
        {
            return shortcut.IsBound() ? shortcut.ToString() : "unbound";
        }

        /// <summary>Anchors (or un-anchors) the top-row inventory slot under the mouse.</summary>
        private static void TrySetAnchorUnderCursor()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || gui.m_playerGrid == null)
            {
                return;
            }

            InventoryGrid.Element element = gui.m_playerGrid.GetHoveredElement();
            if (element == null)
            {
                return;
            }

            Vector2i pos = element.m_pos;
            if (pos.y != 0)
            {
                // Only the hotbar row can be anchored — the rest of the bag has no hotkey.
                return;
            }

            int slot = pos.x + 1;

            if (ModConfig.AnchorSlot.Value == slot)
            {
                ModConfig.AnchorSlot.Value = 0;
                Notifier.Show("Quick Swap: anchor cleared");
                Log.LogInfo("Anchor cleared.");
            }
            else
            {
                ModConfig.AnchorSlot.Value = slot;
                Notifier.Show("Quick Swap: anchored to slot " + slot);
                Log.LogInfo("Anchored to slot " + slot + ".");
            }
        }
    }
}
