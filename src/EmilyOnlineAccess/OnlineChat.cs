using System;
using System.Collections.Generic;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // Used by the shared navigator: Enter is the game's Send while a message is ready.
    internal static class ChatState
    {
        public static bool EnterSendsNow()
        {
            ChatManagerBehavior m = OnlineChat.Window;
            return m != null && m.IsShowing && m.sendButton.IsShowing;
        }
    }

    // What the mod keeps about Emily Online's chats: each buddy's lines for reading back.
    internal static class OnlineChat
    {
        private static readonly Dictionary<ulong, List<string>> Lines = new Dictionary<ulong, List<string>>();
        public static int ReviewAt = -1;

        public static ChatManagerBehavior Window
        {
            get
            {
                ApplicationManagerBehavior app = ApplicationManagerBehavior.Instance;
                return app != null ? app.chatManager : null;
            }
        }

        public static bool IsOpen(CSteamID id)
        {
            ChatManagerBehavior m = Window;
            return m != null && m.IsShowing && m.CurrentBuddy == id;
        }

        public static List<string> LinesFor(CSteamID id)
        {
            List<string> l;
            if (!Lines.TryGetValue(id.m_SteamID, out l)) { l = new List<string>(); Lines[id.m_SteamID] = l; }
            return l;
        }

        public static List<string> OpenLines()
        {
            ChatManagerBehavior m = Window;
            return m != null && m.IsShowing ? LinesFor(m.CurrentBuddy) : null;
        }

        public static string BuddyName()
        {
            ChatManagerBehavior m = Window;
            if (m == null) return "";
            FriendStruct f = Traverse.Create(m).Field("buddyData").GetValue<FriendStruct>();
            return f != null ? f.name : "";
        }

        public static string StateName(FriendState s)
        {
            switch (s)
            {
                case FriendState.InGame: return "in Emily Online";
                case FriendState.Online: return "online";
                case FriendState.Busy: return "busy";
                case FriendState.Away: return "away";
                case FriendState.Snooze: return "snoozing";
                default: return "offline";
            }
        }

        public static void SayWhere()
        {
            ChatManagerBehavior m = Window;
            if (m == null || !m.IsShowing) { Tell.Now("No chat open. Use Up and Down to find a buddy in the buddy list."); return; }
            FriendStruct f = Traverse.Create(m).Field("buddyData").GetValue<FriendStruct>();
            string typed = Tell.Plain(m.textWriter.ContentText);
            Tell.Now("Chatting with " + f.name + ", " + StateName(f.state) + ". "
                     + (typed.Length > 0 ? "Your message so far: " + typed + "." : "Nothing typed yet."));
        }

        public static void OpenBuddyProfile()
        {
            ChatManagerBehavior m = Window;
            if (m == null || !m.IsShowing) { Tell.Now("Open a chat first."); return; }
            m.DidPressOtherBuddyInfo();
        }

        // A line was added to a buddy's chat history (any message, status or warning).
        public static void Stored(CSteamID friend, ChatMessageDehydrated msg)
        {
            string text = LineText(msg);
            if (text.Length == 0) return;
            LinesFor(friend).Add(text);
            bool open = IsOpen(friend);
            if (open) ReviewAt = -1;

            switch (msg.messageType)
            {
                case ChatMessageType.PlayerMessage:
                    if (Settings.OwnSent.On) Tell.Queue(text);
                    else Tell.Queue("Sent.");
                    return;
                case ChatMessageType.OtherMessage:
                    if (open || Settings.OtherChats.On) Tell.Queue(text);
                    return;
                case ChatMessageType.SystemSignIn:
                case ChatMessageType.SystemSignOut:
                    if (Settings.Statuses.Index == 2 || (Settings.Statuses.Index == 1 && open)) Tell.Queue(text);
                    return;
                default:   // warnings and reminders
                    Tell.Queue(text);
                    return;
            }
        }

        public static string LineText(ChatMessageDehydrated m)
        {
            string body = Tell.Plain(m.message);
            switch (m.messageType)
            {
                case ChatMessageType.PlayerMessage:
                case ChatMessageType.OtherMessage:
                    return Tell.Plain(m.username) + ": " + body;
                case ChatMessageType.SystemSignIn:
                case ChatMessageType.SystemSignOut:
                    return Tell.Stop(Tell.Plain(m.username) + " " + body);
                default:
                    return Tell.Stop(body);
            }
        }
    }

    [HarmonyPatch]
    internal static class OnlineChatPatches
    {
        // Every chat line (sent, received, statuses, warnings) is stored here.
        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), "StoreChatMessage")]
        private static void Stored(CSteamID friend, ChatMessageDehydrated chatMessage)
        {
            try { OnlineChat.Stored(friend, chatMessage); }
            catch (Exception e) { Plugin.Log.LogError("Chat line: " + e); }
        }

        // Opening a buddy's chat (buddy list).
        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowChat))]
        private static void ChatOpened(CSteamID id)
        {
            ChatManagerBehavior m = OnlineChat.Window;
            FriendStruct f = Traverse.Create(m).Field("buddyData").GetValue<FriendStruct>();
            List<string> lines = OnlineChat.LinesFor(id);
            OnlineChat.ReviewAt = -1;
            string last = lines.Count > 0 ? " Last line: " + lines[lines.Count - 1] : " No messages yet.";
            string typed = Tell.Plain(m.textWriter.ContentText);
            Tell.Now("Chat with " + f.name + ", " + OnlineChat.StateName(f.state) + "." + last
                     + (typed.Length > 0 ? " You were typing: " + typed + "." : "")
                     + " Type your message and press Enter to send.");
        }

        // "is typing..."
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.ShowTypingAction))]
        private static void Typing(ChatManagerBehavior __instance)
        {
            ScreenWatch.Later(0.05f, () =>
            {
                string t = Ui.Text(__instance.typingLabel);
                if (t.Length > 0 && Settings.BuddyTyping.On) Tell.Queue(t);
            });
        }

        // Your warning level (when a buddy warns you) shows under the chat.
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.RateLimitPlayer))]
        private static void RateLimited() { Tell.Queue("You can't send messages for a while: you've been warned too much."); }
    }
}
