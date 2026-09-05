namespace QuickSwap
{
    /// <summary>
    /// Mirrors the guard list Valheim's own <c>HotkeyBar.Update</c> uses, so our hotkeys
    /// are ignored in exactly the situations the vanilla hotbar keys are.
    /// </summary>
    internal static class GameGuards
    {
        internal static bool AcceptsGameplayInput()
        {
            if (InventoryGui.IsVisible()) return false;
            if (Menu.IsVisible()) return false;
            if (global::Console.IsVisible()) return false;
            if (TextInput.IsVisible()) return false;
            if (StoreGui.IsVisible()) return false;
            if (Minimap.IsOpen()) return false;
            if (GameCamera.InFreeFly()) return false;
            if (Hud.IsPieceSelectionVisible()) return false;
            if (Hud.InRadial()) return false;
            if (PlayerCustomizaton.IsBarberGuiVisible()) return false;

            Chat chat = Chat.instance;
            if (chat != null && chat.HasFocus()) return false;

            return true;
        }
    }
}
