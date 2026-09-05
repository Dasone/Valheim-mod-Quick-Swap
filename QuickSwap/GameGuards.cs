namespace QuickSwap
{
    internal static class GameGuards
    {
        /// <summary>
        /// Whether the player is in a state where a hotbar key should do something.
        /// </summary>
        /// <remarks>
        /// <c>Player.TakeInput</c> is the game's own answer to "can this player act right
        /// now" — it is what gates walking, attacking and the vanilla hotbar keys. Using it
        /// rather than a hand-rolled list means chat, the inventory, the map, menus, text
        /// viewers, death, cutscenes and teleporting are all covered, and stay covered if
        /// the game adds another case. The two extras below are UI states Valheim checks
        /// separately in <c>HotkeyBar.Update</c>.
        /// </remarks>
        internal static bool AcceptsGameplayInput(Player player)
        {
            if (player == null || !player.TakeInput())
            {
                return false;
            }

            if (Hud.IsPieceSelectionVisible() || Hud.InRadial())
            {
                return false;
            }

            return true;
        }
    }
}
