namespace QuickSwap
{
    /// <summary>
    /// Remembers which hotbar slots the local player used, and drives the swap actions.
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

        /// <summary>
        /// The main swap. With a slot anchored it toggles between the anchor and the last
        /// other slot you used — anchor 8, select 1, and it swaps 8 and 1; select 2, and it
        /// swaps 8 and 2. With nothing anchored it toggles the last two slots used.
        /// </summary>
        internal static void QuickSwap()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            SeedFromEquipped(player);

            int anchor = ModConfig.AnchorSlot.Value;
            if (anchor > 0 && ModConfig.AnchorTakesPriority.Value)
            {
                SwapAgainst(anchor);
                return;
            }

            Activate(Previous);
        }

        /// <summary>
        /// The anchor-only swap. Same toggle as <see cref="QuickSwap"/> when an anchor is
        /// set, but never falls back to the last-two-slots behaviour, so a key bound to
        /// this stays reserved for the anchor.
        /// </summary>
        internal static void AnchorSwap()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            SeedFromEquipped(player);

            int anchor = ModConfig.AnchorSlot.Value;
            if (anchor <= 0)
            {
                Notifier.Show("Quick Swap: no anchored slot. Use the set anchor bind on a hotbar slot in your inventory.");
                return;
            }

            SwapAgainst(anchor);
        }

        /// <summary>
        /// Toggles with <paramref name="anchor"/> permanently as one end: off the anchor,
        /// go to it; on it, go back to the last other slot.
        /// </summary>
        private static void SwapAgainst(int anchor)
        {
            if (Current != anchor)
            {
                Activate(anchor);
                return;
            }

            if (Previous <= 0)
            {
                Notifier.Show("Quick Swap: no other slot used yet to swap back to.");
                return;
            }

            Activate(Previous);
        }

        /// <summary>
        /// Adopts the currently equipped hotbar slot when there is no history yet.
        /// </summary>
        /// <remarks>
        /// Without this, an empty history plus an anchor is a dead end: the first press
        /// jumps to the anchor with nothing recorded behind it, so the press after that has
        /// nowhere to go and the key looks broken. That state is not exotic — it is every
        /// fresh login, and every ScriptEngine hot reload, since the history is static.
        /// </remarks>
        private static void SeedFromEquipped(Player player)
        {
            if (Current > 0)
            {
                return;
            }

            Inventory inventory = player.GetInventory();
            if (inventory == null)
            {
                return;
            }

            int width = inventory.GetWidth();
            for (int x = 0; x < width; x++)
            {
                ItemDrop.ItemData item = inventory.GetItemAt(x, 0);
                if (item != null && item.m_equipped)
                {
                    Current = x + 1;
                    return;
                }
            }
        }

        private static void Activate(int slot)
        {
            Player player = Player.m_localPlayer;
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
