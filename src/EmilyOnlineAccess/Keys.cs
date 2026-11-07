using System;
using EmilyIsAwayTooAccess.Speech;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // The mod's keys in Emily Online. The chat box has the keyboard most of the time, so the
    // letter keys work with Ctrl (and on their own when no text box is focused):
    //   Page Up / Page Down       previous / next line of the open chat
    //   Ctrl+Home / Ctrl+End      first / latest line
    //   Ctrl+R  (or R)            repeat the last thing the mod said
    //   Ctrl+W  (or W)            who you're chatting with, and what you've typed so far
    //   Ctrl+I  (or I)            open your buddy's profile
    //   Ctrl+M  (or M)            mod settings
    internal class Keys : MonoBehaviour
    {
        private static int _usedFrame = -1;
        public static bool ModUsedKeyThisFrame => _usedFrame == Time.frameCount;
        public static void UseKey() { _usedFrame = Time.frameCount; }

        public static bool Ctrl() { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); }

        public static bool TextEntryActive() { return Navigator.FocusedBox() != null; }

        private void Update()
        {
            try
            {
                if (SettingsMenu.HoldsKeys) return;
                if (Input.GetKeyDown(KeyCode.PageUp)) Review(-1);
                else if (Input.GetKeyDown(KeyCode.PageDown)) Review(1);
                else if (Input.GetKeyDown(KeyCode.End) && Ctrl()) Review(int.MaxValue);
                else if (Input.GetKeyDown(KeyCode.Home) && Ctrl()) Review(int.MinValue);
                if (!Ctrl() && TextEntryActive()) return;
                if (Input.GetKeyDown(KeyCode.R)) Tell.Now(Tell.Last);
                else if (Input.GetKeyDown(KeyCode.W)) OnlineChat.SayWhere();
                else if (Input.GetKeyDown(KeyCode.I)) OnlineChat.OpenBuddyProfile();
            }
            catch (Exception e) { Plugin.Log.LogError("Keys: " + e); }
        }

        private static void Review(int by)
        {
            var lines = OnlineChat.OpenLines();
            if (lines == null || lines.Count == 0) { Tell.Now("No chat lines yet."); return; }
            int n = lines.Count, i;
            if (by == int.MaxValue) i = n - 1;
            else if (by == int.MinValue) i = 0;
            else i = (OnlineChat.ReviewAt < 0 ? n : OnlineChat.ReviewAt) + by;
            string edge = "";
            if (i < 0) { i = 0; edge = "Top. "; }
            if (i >= n) { i = n - 1; edge = "Bottom. "; }
            OnlineChat.ReviewAt = i;
            Tell.Now(edge + lines[i]);
        }

        public static string SayChar(char c)
        {
            if (c == ' ') return "space";
            if (char.IsUpper(c)) return "cap " + c;
            return c.ToString();
        }
    }
}
