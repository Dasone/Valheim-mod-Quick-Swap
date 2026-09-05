namespace QuickSwap
{
    /// <summary>
    /// Remembers which hotbar slots the local player used, and drives the two swap actions.
    /// Slot numbers are 1-based to match the on-screen hotbar; 0 means "nothing recorded".
    /// </summary>
    internal static class SwapController
    {
        /// <summary>Slot the player is on right now.</summary>
        internal static int Current { get; private set; }

        /// <summary>Slot the player was on before <see cref="Current"/>.</summary>
        internal static int Previous { get; private set; }

        internal static void Reset()
        {
            Current = 0;
            Previous = 0;
        }

        /// <summary>Called from the <see cref="Player.UseHotbarItem"/> patch after a slot is actually used.</summary>
        internal static void Record(int slot)
        {
            if (slot <= 0 || slot == Current)
            {
                return;
            }

            Previous = Current;
            Current = slot;
        }

        /// <summary>Toggle back to the previously used slot. Pressing again returns you here.</summary>
        internal static void QuickSwap()
        {
            Activate(Previous);
        }

        /// <summary>
        /// The anchor-aware swap. With an anchor set this toggles between the anchored slot
        /// and the last other slot you used - anchor 2, work on 3 then 4, and this swaps
        /// 2 and 4. With no anchor set it degrades to the plain <see cref="QuickSwap"/>,
        /// so a single key covers both situations.
        /// </summary>
        internal static void AnchorQuickSwap()
        {
            if (ModConfig.AnchorSlot.Value <= 0)
            {
                QuickSwap();
                return;
            }

            AnchorSwap();
        }

        /// <summary>
        /// Jump to the anchored slot, or — if already on it — back to wherever you came from.
        /// Because <see cref="Record"/> runs on the way in, "where you came from" is just
        /// <see cref="Previous"/>, which is what makes 3 -> anchor -> 3, 4 -> anchor -> 4 work.
        /// </summary>
        internal static void AnchorSwap()
        {
            int anchor = ModConfig.AnchorSlot.Value;
            if (anchor <= 0)
            {
                Notifier.Show("Quick Swap: no anchored slot. Use the set anchor bind on a hotbar slot in your inventory.");
                return;
            }

            Activate(Current == anchor ? Previous : anchor);
        }

        private static void Activate(int slot)
        {
            Player player = global::Player.m_localPlayer;
            if (player == null || slot <= 0)
            {
                return;
            }

            Inventory inventory = player.GetInventory();
            if (inventory == null || slot > inventory.GetWidth())
            {
                return;
            }

            // An empty slot would be a no-op in the game, so skip it rather than
            // let the swap history drift out of sync with what the player sees.
            if (inventory.GetItemAt(slot - 1, 0) == null)
            {
                return;
            }

            player.UseHotbarItem(slot);
        }
    }
}
