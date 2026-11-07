using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // When your reply is being typed, the game counts any key at all as "type the next
    // letter" (and holding any key types on and on). That includes keys that belong to the
    // screen reader or Windows: the NVDA key (Insert or Caps Lock), Shift, Ctrl, Alt, the
    // Windows key, the F keys and so on. In NVDA's sleep mode those reach the game, so
    // NVDA commands typed letters instead. The game's key check is narrowed here so only
    // real typing keys count; the others are left alone for the screen reader.
    [HarmonyPatch(typeof(ChatManagerBehavior), "InputKey")]
    internal static class TypingKeys
    {
        private static readonly HashSet<KeyCode> NotTyping = new HashSet<KeyCode>
        {
            KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl,
            KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.AltGr,
            KeyCode.LeftWindows, KeyCode.RightWindows, KeyCode.LeftCommand, KeyCode.RightCommand,
            KeyCode.Insert, KeyCode.CapsLock, KeyCode.Numlock, KeyCode.ScrollLock,
            KeyCode.Print, KeyCode.SysReq, KeyCode.Break, KeyCode.Pause, KeyCode.Menu, KeyCode.Help,
            KeyCode.Escape, KeyCode.Tab,
            KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8,
            KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12, KeyCode.F13, KeyCode.F14, KeyCode.F15,
        };

        // Every keyboard key that counts as typing (mouse and joystick buttons excluded).
        private static readonly List<KeyCode> Typing = Build();

        private static List<KeyCode> Build()
        {
            var list = new List<KeyCode>();
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                if (k != KeyCode.None && (int)k < (int)KeyCode.Mouse0 && !NotTyping.Contains(k) && !list.Contains(k)) list.Add(k);
            return list;
        }

        private static bool AnyTypingKey(bool down)
        {
            foreach (KeyCode k in Typing)
                if (down ? Input.GetKeyDown(k) : Input.GetKey(k)) return true;
            return false;
        }

        // isDown: a key went down this frame; otherwise: a key is being held.
        private static void Postfix(bool isDown, ref bool __result)
        {
            if (__result) __result = AnyTypingKey(isDown);
        }
    }
}
