using BepInEx.Configuration;
using UnityEngine;

namespace QuickSwap
{
    internal static class Shortcuts
    {
        /// <summary>
        /// True on the frame the shortcut's main key goes down with its modifiers held.
        /// </summary>
        /// <remarks>
        /// Deliberately not <see cref="KeyboardShortcut.IsDown"/>. That one also requires
        /// that no other key on the keyboard is held, which is right for a settings-menu
        /// chord but wrong for a gameplay bind: holding W to run would silently kill a
        /// bare Q shortcut. Only the keys the shortcut actually names are checked here.
        /// Mouse buttons are unaffected either way — BepInEx already excludes them.
        /// </remarks>
        internal static bool Triggered(this KeyboardShortcut shortcut)
        {
            KeyCode mainKey = shortcut.MainKey;
            if (mainKey == KeyCode.None || !Input.GetKeyDown(mainKey))
            {
                return false;
            }

            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (!Input.GetKey(modifier))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
