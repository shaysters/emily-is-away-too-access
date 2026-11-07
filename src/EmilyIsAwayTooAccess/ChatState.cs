using System.Collections.Generic;
using System.Text;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // What the mod knows about one chat window (Emily, Evelyn, or a secret buddy).
    internal class Chat
    {
        public ChatManagerBehavior Window;
        public readonly List<string> Lines = new List<string>();
        public int ReviewAt = -1;
        public string[] Choices = new string[0];
        public bool ChoicesUp;
        public bool FreeTyping;
        public bool JustFreeTyped;
        public bool Writing;              // a chosen reply is being typed (until it's sent)        // the next line you send is the answer you typed
        public float TimerEnd;            // Time.time the reply timer runs out, 0 = none
        public int TimerWarned;
        public bool Unread;               // a message came while you were in the other window
        public int LinkAt = -1;           // link picked with Shift+L, -1 = the newest
        public readonly List<KeyValuePair<string, string>> Links = new List<KeyValuePair<string, string>>();  // text, url

        public string Buddy
        {
            get
            {
                BaseScriptBehavior s = Traverse.Create(Window).Property("Script").GetValue<BaseScriptBehavior>();
                return s != null ? s.OtherUsername : "chat";
            }
        }

        public bool Showing => Window != null && Window.IsShowing;
        public bool ReadyToSend => Window.sendButton != null && Window.sendButton.IsShowing;
        public float TimeLeft => TimerEnd > 0 ? Mathf.Max(0f, TimerEnd - Time.time) : 0f;
    }

    internal static class ChatState
    {
        private static readonly Dictionary<ChatManagerBehavior, Chat> Chats = new Dictionary<ChatManagerBehavior, Chat>();
        private static Chat _lastViewed;

        public static Chat For(ChatManagerBehavior m)
        {
            if (m == null) return null;
            Chat c;
            if (!Chats.TryGetValue(m, out c))
            {
                // Windows from a scene that's gone are destroyed: drop them.
                var dead = new List<ChatManagerBehavior>();
                foreach (var k in Chats.Keys) if (k == null) dead.Add(k);
                foreach (var k in dead) Chats.Remove(k);
                c = new Chat { Window = m };
                Chats[m] = c;
            }
            return c;
        }

        public static IEnumerable<Chat> All()
        {
            ApplicationManagerBehavior app = ApplicationManagerBehavior.Instance;
            if (app == null) yield break;
            foreach (ChatManagerBehavior m in new[] { app.chatManager1, app.chatManager2, app.chatManager3 })
                if (m != null) yield return For(m);
        }

        // The chat window on screen, or the last one that was.
        public static Chat Viewed()
        {
            foreach (Chat c in All()) if (c.Showing) { _lastViewed = c; return c; }
            return _lastViewed != null && _lastViewed.Window != null ? _lastViewed : null;
        }

        public static Chat Showing()
        {
            foreach (Chat c in All()) if (c.Showing) return c;
            return null;
        }

        // Enter belongs to the game when a finished reply is waiting in the window on screen.
        public static bool EnterSendsNow()
        {
            Chat c = Showing();
            return c != null && c.ReadyToSend;
        }

        public static bool FreeTyping()
        {
            Chat c = Showing();
            return c != null && c.FreeTyping;
        }

        public static string ChoiceText(Chat c)
        {
            var sb = new StringBuilder();
            foreach (string s in c.Choices) { if (sb.Length > 0) sb.Append(" "); sb.Append(Tell.Stop(s)); }
            return sb.ToString();
        }

        public static void SayChoices()
        {
            Chat c = Showing();
            if (c != null && c.ChoicesUp) { Tell.Now("Reply with " + ChoiceText(c)); return; }
            foreach (Chat o in All())
                if (o.ChoicesUp) { Tell.Now(o.Buddy + " is waiting for your reply in the other window. Press Tab."); return; }
            Tell.Now("No reply choices right now.");
        }

        public static void SayTimers()
        {
            var sb = new StringBuilder();
            foreach (Chat c in All())
                if (c.TimerEnd > 0) sb.Append(c.Buddy + ": " + Mathf.CeilToInt(c.TimeLeft) + " seconds left. ");
            Tell.Now(sb.Length > 0 ? sb.ToString() : "No reply timer running.");
        }

        // Which window is on screen and what's going on in each one.
        public static void SayWhere()
        {
            var sb = new StringBuilder();
            Chat on = Showing();
            if (on != null) sb.Append("In " + on.Buddy + "'s window. ");
            else sb.Append(ScreenWatch.WhereText() + ". ");
            foreach (Chat c in All())
            {
                if (c.Lines.Count == 0 && !c.ChoicesUp) continue;
                sb.Append(c.Buddy + ": " + State(c) + ". ");
            }
            Tell.Now(sb.ToString());
        }

        public static string State(Chat c)
        {
            if (c.ChoicesUp) return "waiting for your reply" + (c.TimerEnd > 0 ? ", " + Mathf.CeilToInt(c.TimeLeft) + " seconds left" : "");
            if (c.FreeTyping) return "type your answer, then press Enter";
            if (c.ReadyToSend) return "your reply is ready, press Enter to send";
            if (c.Writing) return "your reply isn't finished, keep pressing keys to type it";
            string typing = Ui.Text(c.Window.typingLabel);
            if (typing.Length > 0) return typing;
            if (c.Unread) return "new messages";
            return c.Lines.Count > 0 ? "last message, " + c.Lines[c.Lines.Count - 1] : "nothing yet";
        }

        // Called when the player switches windows (buddy list or Tab).
        public static void Switched(ChatManagerBehavior m)
        {
            Chat c = For(m);
            _lastViewed = c;
            c.ReviewAt = -1;
            string what;
            if (c.ChoicesUp) what = "Reply with " + ChoiceText(c);
            else if (c.Unread && c.Lines.Count > 0) what = c.Lines[c.Lines.Count - 1];
            else what = State(c);
            c.Unread = false;
            UnreadSaid.Remove(c);
            Tell.Now(c.Buddy + " window. " + Tell.Stop(what));
        }

        public static void OpenLatestLink()
        {
            Chat c = Viewed();
            if (c == null || c.Links.Count == 0) { Tell.Now("No links in this chat."); return; }
            var link = c.Links[c.LinkAt >= 0 && c.LinkAt < c.Links.Count ? c.LinkAt : c.Links.Count - 1];
            c.LinkAt = -1;
            SoundManagerBehavior.Instance?.PlayMouseClick();
            BaseMasterScriptBehavior.Instance?.PlayerClickedLink(link.Value);
            Tell.Now("Opening " + link.Key + " in your web browser.");
            Application.OpenURL(link.Value);
        }

        // Shift+L: step back through older links; L then opens the one you're on.
        public static void PickOlderLink()
        {
            Chat c = Viewed();
            if (c == null || c.Links.Count == 0) { Tell.Now("No links in this chat."); return; }
            int i = (c.LinkAt < 0 ? c.Links.Count : c.LinkAt) - 1;
            if (i < 0) i = c.Links.Count - 1;   // wrap to the newest
            c.LinkAt = i;
            Tell.Now("Link " + (i + 1) + " of " + c.Links.Count + ": " + c.Links[i].Key + ". Press L to open it.");
        }

        public static void OpenBuddyProfile()
        {
            Chat c = Showing();
            if (c == null) { Tell.Now("Open a chat window first."); return; }
            c.Window.DidPressOtherBuddyInfo();
        }

        // A new line in one of the chats.
        // spoken: what to say instead of the line itself ("" = nothing; null = the line).
        public static void NewLine(Chat c, string text, string url, string spoken = null)
        {
            c.Lines.Add(text);
            if (!string.IsNullOrEmpty(url)) { c.Links.Add(new KeyValuePair<string, string>(text, url)); c.LinkAt = -1; }
            if (spoken == "") return;
            bool here = c.Showing || Showing() == null;
            if (!here) c.Unread = true;
            if (here || Settings.OtherWindow.On) Tell.Queue(spoken ?? text);
            else if (UnreadSaid.Add(c)) Tell.Queue("New message from " + c.Buddy + ".");   // once until you look
        }

        private static readonly HashSet<Chat> UnreadSaid = new HashSet<Chat>();
    }
}
