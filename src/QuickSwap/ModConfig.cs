using BepInEx.Configuration;
using UnityEngine;

namespace QuickSwap
{
    /// <summary>
    /// Every setting the mod exposes. Rendered by ConfigurationManager (F1) and
    /// written to BepInEx/config/samuel.haggren.quickswap.cfg.
    /// </summary>
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;

        internal static ConfigEntry<KeyboardShortcut> QuickSwapKey;
        internal static ConfigEntry<KeyboardShortcut> AnchorSwapKey;
        internal static ConfigEntry<KeyboardShortcut> SetAnchorBind;

        internal static ConfigEntry<int> AnchorSlot;
        internal static ConfigEntry<bool> ShowAnchorMarker;
        internal static ConfigEntry<string> AnchorMarkerColor;

        internal static ConfigEntry<bool> OnlyTrackEquipment;
        internal static ConfigEntry<bool> ShowMessages;

        private static Color _cachedColor = new Color(1f, 0.82f, 0.29f, 0.95f);
        private static string _cachedColorText;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "1 - General", "Enabled", true,
                new ConfigDescription("Master switch. Turn off to disable all Quick Swap hotkeys without uninstalling.",
                    null, new ConfigurationManagerAttributes { Order = 100 }));

            QuickSwapKey = config.Bind(
                "2 - Keys", "Quick swap key", new KeyboardShortcut(KeyCode.Q),
                new ConfigDescription("Toggles between the last two hotbar slots you used. Press once to go back, press again to return.",
                    null, new ConfigurationManagerAttributes { Order = 90 }));

            AnchorSwapKey = config.Bind(
                "2 - Keys", "Anchor swap key", new KeyboardShortcut(KeyCode.Alpha9),
                new ConfigDescription("Jumps to the anchored slot. Press again to go back to the slot you came from.",
                    null, new ConfigurationManagerAttributes { Order = 80 }));

            SetAnchorBind = config.Bind(
                "2 - Keys", "Set anchor bind", new KeyboardShortcut(KeyCode.Mouse2),
                new ConfigDescription("Press this while hovering a top-row hotbar slot in the open inventory to anchor it. Press it on the anchored slot again to clear the anchor. Default is middle mouse button.",
                    null, new ConfigurationManagerAttributes { Order = 70 }));

            AnchorSlot = config.Bind(
                "3 - Anchor", "Anchored slot", 0,
                new ConfigDescription("Hotbar slot the anchor key jumps to. 0 means no anchor set. Usually set in-game by middle-clicking a hotbar slot.",
                    new AcceptableValueRange<int>(0, 8), new ConfigurationManagerAttributes { Order = 60 }));

            ShowAnchorMarker = config.Bind(
                "3 - Anchor", "Show marker on hotbar", true,
                new ConfigDescription("Draw a coloured bar under the anchored slot on the on-screen hotbar.",
                    null, new ConfigurationManagerAttributes { Order = 50 }));

            AnchorMarkerColor = config.Bind(
                "3 - Anchor", "Marker colour", "#FFD24AF2",
                new ConfigDescription("Colour of the anchor marker, as #RRGGBB or #RRGGBBAA.",
                    null, new ConfigurationManagerAttributes { Order = 40 }));

            OnlyTrackEquipment = config.Bind(
                "4 - Behaviour", "Only remember equipment", true,
                new ConfigDescription("When on, using a consumable (food, mead, arrows) does not overwrite your swap history, so the swap key keeps toggling between your actual tools and weapons.",
                    null, new ConfigurationManagerAttributes { Order = 30 }));

            ShowMessages = config.Bind(
                "4 - Behaviour", "Show HUD messages", true,
                new ConfigDescription("Show a short message in the top-left corner when the anchored slot changes.",
                    null, new ConfigurationManagerAttributes { Order = 20 }));
        }

        /// <summary>Parsed <see cref="AnchorMarkerColor"/>, falling back to amber on a malformed value.</summary>
        internal static Color MarkerColor()
        {
            string text = AnchorMarkerColor.Value;
            if (text == _cachedColorText)
            {
                return _cachedColor;
            }

            _cachedColorText = text;
            if (!string.IsNullOrEmpty(text) && ColorUtility.TryParseHtmlString(text, out Color parsed))
            {
                _cachedColor = parsed;
            }

            return _cachedColor;
        }
    }
}
