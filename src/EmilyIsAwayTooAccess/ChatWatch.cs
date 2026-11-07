using System;
using System.Collections.Generic;
using EmilyIsAwayTooAccess.Speech;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // Watches the chat windows every frame for things the game only draws:
    // your reply as it's typed (and erased), "is typing...", and reply timers running out.
    internal class ChatWatch : MonoBehaviour
    {
        private class Typing
        {
            public string Shown = "";       // text in the reply box last frame
            public string Erased = "";      // letters erased since the last one typed
            public bool Ready;
            public string Label = "";       // "punk4eva is typing..." last frame
            public float LabelSaid = -10f;
        }

        private readonly Dictionary<ChatManagerBehavior, Typing> _typing = new Dictionary<ChatManagerBehavior, Typing>();

        private void Update()
        {
            try
            {
                foreach (Chat c in ChatState.All())
                {
                    Typing t;
                    if (!_typing.TryGetValue(c.Window, out t)) { t = new Typing(); _typing[c.Window] = t; }
                    WatchReply(c, t);
                    WatchLabel(c, t);
                    WatchTimer(c);
                }
            }
            catch (Exception e) { Plugin.Log.LogError("ChatWatch: " + e); }
        }

        private static void WatchReply(Chat c, Typing t)
        {
            string now = c.Window.textWriter != null ? c.Window.textWriter.ContentText : "";
            string old = t.Shown;
            bool ready = c.ReadyToSend;
            if (now == old && ready == t.Ready) return;
            t.Shown = now;

            if (c.FreeTyping)
            {
                if (now.Length == old.Length + 1 && now.StartsWith(old)) Tell.Now(Keys.SayChar(now[now.Length - 1]));
                else if (old.Length == now.Length + 1 && old.StartsWith(now)) Tell.Now(Keys.SayChar(old[old.Length - 1]) + " erased");
                else Tell.Now(now.Length == 0 ? "blank" : now);
                t.Ready = ready;
                return;
            }

            if (now.Length == 0) { t.Erased = ""; t.Ready = ready; return; }   // sent or cleared

            if (now.Length < old.Length && old.StartsWith(now))
            {
                t.Erased = old.Substring(now.Length) + t.Erased;   // backspaces go right to left
            }
            else if (now.Length > old.Length && now.StartsWith(old))
            {
                if (t.Erased.Length > 0)
                {
                    if (Settings.ReadErased.On) Tell.Queue("erased " + t.Erased.Trim());
                    t.Erased = "";
                }
                char added = now[now.Length - 1];
                char before = now.Length > 1 ? now[now.Length - 2] : ' ';
                // A word is finished when a space or punctuation follows a letter ("jc," reads
                // "jc" once, not again at the space after the comma).
                if (Settings.ReadWords.On && IsBreak(added) && !IsBreak(before))
                {
                    string word = LastWord(now.Substring(0, now.Length - 1));
                    if (word.Length > 0) Tell.Queue(word);
                }
            }

            if (ready && !t.Ready)
            {
                if (t.Erased.Length > 0 && Settings.ReadErased.On) Tell.Queue("erased " + t.Erased.Trim());
                t.Erased = "";
                if (Settings.ReadWords.On && !Settings.ReadReady.On)
                {
                    string word = LastWord(now);
                    if (word.Length > 0) Tell.Queue(word);
                }
                Tell.Queue(Settings.ReadReady.On ? "Ready to send: " + Tell.Stop(now) + " Press Enter." : "Ready to send. Press Enter.");
            }
            t.Ready = ready;
        }

        private static bool IsBreak(char c) { return c == ' ' || c == ',' || c == '.' || c == '?' || c == '!'; }

        private static string LastWord(string s)
        {
            s = s.TrimEnd(' ', ',', '.', '?', '!');
            int sp = s.LastIndexOf(' ');
            return sp < 0 ? s : s.Substring(sp + 1);
        }

        // "punk4eva is typing...", "is deleting...", "transferring to desktop (12kb/26kb)".
        private static void WatchLabel(Chat c, Typing t)
        {
            string now = Ui.Text(c.Window.typingLabel);
            if (now.StartsWith("transferring")) now = "transferring the file to your desktop";
            if (now == t.Label) return;
            t.Label = now;
            if (now.Length == 0 || !Settings.BuddyTyping.On) return;
            if (!c.Showing && !Settings.OtherWindow.On) return;
            if (now.EndsWith("typing...") && Time.unscaledTime - t.LabelSaid < 1.5f) return;   // no stutter
            t.LabelSaid = Time.unscaledTime;
            Tell.Queue(now);
        }

        private static void WatchTimer(Chat c)
        {
            if (c.TimerEnd <= 0 || !Settings.TimerWarnings.On) return;
            float left = c.TimeLeft;
            int mark = left <= 5f ? 2 : left <= 10f ? 1 : 0;
            if (mark <= c.TimerWarned) return;
            c.TimerWarned = mark;
            Tell.Queue((c.Showing ? "" : c.Buddy + ", ") + (mark == 2 ? "5" : "10") + " seconds left.");
        }
    }
}
