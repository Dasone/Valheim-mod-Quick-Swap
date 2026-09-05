using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace QuickSwap
{
    /// <summary>
    /// Quick Swap — toggle between the last two hotbar slots you used, plus an
    /// anchored slot you can bounce off from wherever you happen to be.
    /// </summary>
    [BepInPlugin(Guid, Name, Version)]
    [BepInProcess("valheim.exe")]
    public class QuickSwapPlugin : BaseUnityPlugin
    {
        public const string Guid = "samuel.haggren.quickswap";
        public const string Name = "Quick Swap";
        public const string Version = "1.0.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;
        private Player _lastLocalPlayer;

        private void Awake()
        {
            Log = Logger;

            ModConfig.Init(Config);

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(QuickSwapPlugin).Assembly);

            Log.LogInfo(Name + " " + Version + " loaded.");
        }

        /// <summary>
        /// Runs on hot reload as well as on shutdown, so everything this plugin added
        /// to the game has to come back off here — otherwise a reload stacks a second
        /// set of Harmony patches on top of the first.
        /// </summary>
        private void OnDestroy()
        {
            AnchorMarker.RemoveAll();
            _harmony?.UnpatchSelf();
            Config?.Save();

            Log.LogInfo(Name + " unloaded.");
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
                if (ModConfig.SetAnchorBind.Value.IsDown())
                {
                    TrySetAnchorUnderCursor();
                }

                return;
            }

            if (!GameGuards.AcceptsGameplayInput())
            {
                return;
            }

            if (ModConfig.QuickSwapKey.Value.IsDown())
            {
                SwapController.QuickSwap();
            }
            else if (ModConfig.AnchorSwapKey.Value.IsDown())
            {
                SwapController.AnchorSwap();
            }
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
