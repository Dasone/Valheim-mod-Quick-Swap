using HarmonyLib;

namespace QuickSwap.Patches
{
    /// <summary>Repaints the anchor marker whenever the hotbar redraws.</summary>
    [HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
    internal static class HotkeyBar_UpdateIcons_Patch
    {
        private static void Postfix(HotkeyBar __instance)
        {
            AnchorMarker.Apply(__instance);
        }
    }
}
