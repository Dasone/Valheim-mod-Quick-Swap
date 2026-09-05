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
        ///
        /// The cost of that looseness is that a bare Q also fires on alt+Q, so anything
        /// dispatching two shortcuts that share a main key has to try the one with more
        /// modifiers first — see <see cref="ModifierCount"/>.
        /// </remarks>
        internal static bool Triggered(this KeyboardShortcut shortcut)
        {
            if (!shortcut.IsBound() || !Input.GetKeyDown(shortcut.MainKey))
            {
                return false;
            }

            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (!ModifierHeld(modifier))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Whether the shortcut names a key at all.</summary>
        internal static bool IsBound(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None;
        }

        /// <summary>How specific the shortcut is: alt+Q beats a bare Q.</summary>
        internal static int ModifierCount(this KeyboardShortcut shortcut)
        {
            int count = 0;
            foreach (KeyCode unused in shortcut.Modifiers)
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// A shortcut asking for left shift is satisfied by right shift, and the same for
        /// control and alt. Someone binding "shift + Q" does not mean one shift in particular.
        /// </summary>
        private static bool ModifierHeld(KeyCode modifier)
        {
            if (Input.GetKey(modifier))
            {
                return true;
            }

            switch (modifier)
            {
                case KeyCode.LeftShift: return Input.GetKey(KeyCode.RightShift);
                case KeyCode.RightShift: return Input.GetKey(KeyCode.LeftShift);
                case KeyCode.LeftControl: return Input.GetKey(KeyCode.RightControl);
                case KeyCode.RightControl: return Input.GetKey(KeyCode.LeftControl);
                case KeyCode.LeftAlt: return Input.GetKey(KeyCode.RightAlt);
                case KeyCode.RightAlt: return Input.GetKey(KeyCode.LeftAlt);
                default: return false;
            }
        }
    }
}
