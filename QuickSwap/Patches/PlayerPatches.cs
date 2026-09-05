using HarmonyLib;

namespace QuickSwap.Patches
{
    /// <summary>
    /// Records every hotbar slot the local player actually uses. This is the single
    /// source of truth for "last used slot" — it fires for the number keys, for the
    /// gamepad hotbar, and for our own swaps, which is what keeps the toggle honest.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UseHotbarItem))]
    internal static class Player_UseHotbarItem_Patch
    {
        /// <param name="__state">True when this call will really do something worth recording.</param>
        private static void Prefix(Player __instance, int index, out bool __state)
        {
            __state = false;

            if (__instance == null || __instance != Player.m_localPlayer)
            {
                return;
            }

            Inventory inventory = __instance.GetInventory();
            if (inventory == null)
            {
                return;
            }

            ItemDrop.ItemData item = inventory.GetItemAt(index - 1, 0);
            if (item == null)
            {
                // Empty slot: the game does nothing, so neither do we.
                return;
            }

            if (ModConfig.OnlyTrackEquipment.Value && !item.IsEquipable())
            {
                return;
            }

            __state = true;
        }

        private static void Postfix(int index, bool __state)
        {
            if (__state)
            {
                SwapController.Record(index);
            }
        }
    }
}
