using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmilyIsAwayTooAccess
{
    // Emotes are TextMesh Pro sprites written into messages as <sprite=N>. Their names come
    // from the game's sprite asset; the emote buttons are matched by the number they insert.
    internal static class Emotes
    {
        // The game's eight yellow smileys, in sprite order (named by looking at each one;
        // the game itself only calls them "emojis 0" to "emojis 7").
        private static readonly string[] Names =
        {
            "laughing", "yelling", "winking", "tongue out", "cool, with sunglasses", "smiling", "crying", "kiss",
        };

        public static string Name(int index)
        {
            if (index >= 0 && index < Names.Length) return Names[index] + " emote";
            return "emote " + index;
        }

        // The emote a button inserts (its On Click calls InsertEmote with a number), or null.
        public static string ForButton(Component c)
        {
            Button b = c as Button;
            if (b == null) return null;
            int count = b.onClick.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                string method = b.onClick.GetPersistentMethodName(i);
                if (method != "InsertEmote" && method != "DidPressEmoji") continue;
                var calls = Traverse.Create(b.onClick).Field("m_PersistentCalls").Field("m_Calls").GetValue<IList>();
                if (calls == null || i >= calls.Count) return "emote";
                int n = Traverse.Create(calls[i]).Field("m_Arguments").Field("m_IntArgument").GetValue<int>();
                return Name(n);
            }
            return null;
        }
    }
}
