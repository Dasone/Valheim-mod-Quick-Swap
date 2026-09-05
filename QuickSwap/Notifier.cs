namespace QuickSwap
{
    /// <summary>Thin wrapper around the game's message HUD so callers stay null-safe.</summary>
    internal static class Notifier
    {
        internal static void Show(string text)
        {
            if (!ModConfig.ShowMessages.Value || string.IsNullOrEmpty(text))
            {
                return;
            }

            MessageHud messageHud = MessageHud.instance;
            if (messageHud != null)
            {
                messageHud.ShowMessage(MessageHud.MessageType.TopLeft, text);
            }
        }
    }
}
