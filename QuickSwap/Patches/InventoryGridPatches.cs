using HarmonyLib;

namespace QuickSwap.Patches
{
    /// <summary>Repaints the anchor marker whenever the open inventory redraws.</summary>
    [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
    internal static class InventoryGrid_UpdateGui_Patch
    {
        private static void Postfix(InventoryGrid __instance)
        {
            AnchorMarker.Apply(__instance);
        }
    }
}
