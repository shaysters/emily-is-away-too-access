using System;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // The mod's own keys. In a chat every key also types the next letter of your reply
    // (that's how the game works), which is harmless: the reply is already chosen and
    // nothing is sent until you press Enter.
    //   Page Up / Page Down   previous / next line of the chat you're looking at
    //   Ctrl+End              the latest line
    //   C                     repeat the reply choices
    //   R                     repeat the last thing the mod said
    //   T                     time left on a reply timer
    //   W                     which window you're in, and what's waiting in each
    //   L                     open the latest link in this chat (or the one picked with Shift+L)
    //   Shift+L               step back through older links
    //   I                     open your buddy's profile
    //   M                     mod settings
    internal class Keys : MonoBehaviour
    {
        private static int _usedFrame = -1;

        // The navigator or a mod key used this frame's key press; the game's key
        // listeners skip the frame so it doesn't also type a letter.
        public static bool ModUsedKeyThisFrame => _usedFrame == Time.frameCount;
        public static void UseKey() { _usedFrame = Time.frameCount; }

        public static bool Shift() { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }

        public static bool Ctrl() { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); }

        // Typing into a text box or a free-text reply (a nickname): letter keys are text.
        public static bool TextEntryActive()
        {
            return Navigator.FocusedBox() != null || ChatState.FreeTyping() || CodeBoxFocused();
        }

        private static bool CodeBoxFocused()
        {
            CodePopupBehavior p = CodePopupBehavior.Instance;
            return p != null && Traverse.Create(p).Field("isInputFocused").GetValue<bool>();
        }

        private void Update()
        {
            try
            {
                if (SettingsMenu.HoldsKeys) return;
                if (Input.GetKeyDown(KeyCode.PageUp)) Review(-1);
                else if (Input.GetKeyDown(KeyCode.PageDown)) Review(1);
                else if (Input.GetKeyDown(KeyCode.End) && Ctrl()) Review(int.MaxValue);
                else if (Input.GetKeyDown(KeyCode.Home) && Ctrl()) Review(int.MinValue);
                if (TextEntryActive()) return;
                if (Input.GetKeyDown(KeyCode.R)) Tell.Now(Tell.Last);
                else if (Input.GetKeyDown(KeyCode.C)) ChatState.SayChoices();
                else if (Input.GetKeyDown(KeyCode.T)) ChatState.SayTimers();
                else if (Input.GetKeyDown(KeyCode.W)) ChatState.SayWhere();
                else if (Input.GetKeyDown(KeyCode.L) && Shift()) ChatState.PickOlderLink();
                else if (Input.GetKeyDown(KeyCode.L)) ChatState.OpenLatestLink();
                else if (Input.GetKeyDown(KeyCode.I)) ChatState.OpenBuddyProfile();
            }
            catch (Exception e) { Plugin.Log.LogError("Keys: " + e); }
        }

        private static void Review(int by)
        {
            Chat chat = ChatState.Viewed();
            if (chat == null || chat.Lines.Count == 0) { Tell.Now("No chat lines yet."); return; }
            int n = chat.Lines.Count;
            int i;
            if (by == int.MaxValue) i = n - 1;
            else if (by == int.MinValue) i = 0;
            else i = (chat.ReviewAt < 0 ? n : chat.ReviewAt) + by;
            string edge = "";
            if (i < 0) { i = 0; edge = "Top. "; }
            if (i >= n) { i = n - 1; edge = "Bottom. "; }
            chat.ReviewAt = i;
            Tell.Now(edge + chat.Lines[i]);
        }

        public static string SayChar(char c)
        {
            if (c == ' ') return "space";
            if (char.IsUpper(c)) return "cap " + c;
            return c.ToString();
        }
    }
}
