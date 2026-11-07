using System;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // Hooks into the chat windows: new messages, reply choices, timers, captions.
    [HarmonyPatch]
    internal static class ChatPatches
    {
        private static ChatManagerBehavior OwnerOf(Transform t)
        {
            foreach (Chat c in ChatState.All())
                if (c.Window.chatMessageWindow != null && t.IsChildOf(c.Window.chatMessageWindow.transform)) return c.Window;
            return null;
        }

        // Every chat line (messages, sign-ons, away messages, file transfers) is built here.
        // playSound is false when an old chat log is reloaded at a checkpoint.
        [HarmonyPostfix, HarmonyPatch(typeof(ChatMessageBehavior), nameof(ChatMessageBehavior.SetUp))]
        private static void Line(ChatMessageBehavior __instance, string username, string messageText, ChatMessageType type, bool playSound, string data)
        {
            try
            {
                ChatManagerBehavior m = OwnerOf(__instance.transform);
                if (m == null) return;
                Chat c = ChatState.For(m);
                string text = Ui.Text(__instance.message);
                bool hasLink = type == ChatMessageType.OtherFileSent
                    || ((type == ChatMessageType.PlayerMessage || type == ChatMessageType.OtherMessage) && !string.IsNullOrEmpty(data));
                if (type == ChatMessageType.OtherFileTransfer) text += ". Accept or decline the file.";
                if ((type == ChatMessageType.OtherAway || type == ChatMessageType.OtherSignOut || type == ChatMessageType.OtherSignIn) && !string.IsNullOrEmpty(data))
                    text = Tell.Plain(username + " " + messageText) + ". " + (type == ChatMessageType.OtherAway ? "Away message: " : "") + Tell.Plain(data);
                if (!playSound) { c.Lines.Add(text); return; }   // reloaded history: keep it for review only
                string spoken = null;
                if (hasLink)
                {
                    // Read "punk4eva sent a link to youtoob.com" rather than the whole address.
                    string site = Tell.Plain(messageText);
                    int slash = site.IndexOf('/');
                    if (slash > 0) site = site.Substring(0, slash);
                    string who = type == ChatMessageType.PlayerMessage ? "You" : username;
                    spoken = who + " sent a link to " + site + (type == ChatMessageType.PlayerMessage ? "." : ". Press L to open it.");
                }
                else if (type == ChatMessageType.PlayerMessage && c.JustFreeTyped) c.JustFreeTyped = false;   // read your typed answer
                else if (type == ChatMessageType.PlayerMessage && Settings.ReadReady.On)
                    spoken = "";   // you just heard it as "Ready to send"
                ChatState.NewLine(c, text, hasLink ? data : null, spoken);
            }
            catch (Exception e) { Plugin.Log.LogError("Chat line: " + e); }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TextButtonsBehavior), nameof(TextButtonsBehavior.SetUp))]
        private static void ChoicesSet(TextButtonsBehavior __instance, string text1, string text2, string text3)
        {
            Chat c = ChatState.For(__instance.chatManager);
            if (c != null) c.Choices = new[] { Tell.Plain(text1), Tell.Plain(text2), Tell.Plain(text3) };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TextButtonsBehavior), nameof(TextButtonsBehavior.Show))]
        private static void ChoicesShown(TextButtonsBehavior __instance)
        {
            Chat c = ChatState.For(__instance.chatManager);
            if (c == null) return;
            c.ChoicesUp = true;
            if (c.Showing) Tell.Queue("Reply with " + ChatState.ChoiceText(c));
            else if (ChatState.Showing() == null) Tell.Queue(c.Buddy + " is waiting for your reply. Press Tab to open the chat.");
            else Tell.Queue(c.Buddy + " is waiting for your reply. Press Tab to go there.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TextButtonsBehavior), nameof(TextButtonsBehavior.Hide))]
        private static void ChoicesHidden(TextButtonsBehavior __instance)
        {
            Chat c = ChatState.For(__instance.chatManager);
            if (c != null) c.ChoicesUp = false;
        }

        // Reply timers: made longer or switched off by the "Reply timers" setting.
        [HarmonyPrefix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.StartTimer))]
        private static bool TimerStarts(ChatManagerBehavior __instance, ref float time)
        {
            if (__instance.IsHardStopped) return true;
            Chat c = ChatState.For(__instance);
            if (Settings.TimerScale <= 0f) { c.TimerEnd = 0; return false; }
            time *= Settings.TimerScale;
            c.TimerEnd = Time.time + time;
            c.TimerWarned = time <= 7f ? 2 : time <= 13f ? 1 : 0;   // no warning right as it starts
            Tell.Queue((c.Showing ? "" : c.Buddy + ": ") + "timed reply, " + Mathf.RoundToInt(time) + " seconds.");
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.StopTimer))]
        private static void TimerStops(ChatManagerBehavior __instance) { ChatState.For(__instance).TimerEnd = 0; }

        [HarmonyPrefix, HarmonyPatch(typeof(ChatManagerBehavior), "DecisionTimedOut")]
        private static void TimedOut(ChatManagerBehavior __instance)
        {
            Chat c = ChatState.For(__instance);
            c.TimerEnd = 0;
            c.ChoicesUp = false;
            Tell.Queue("Too late, you didn't answer " + c.Buddy + ".");
        }

        // "emily will remember that".
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.ShowReminder))]
        private static void Reminder(string text) { Tell.Queue(Tell.Stop(text)); }

        private static float _savedAt = -10f;
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.ShowCheckpoint))]
        private static void Checkpoint()
        {
            if (Time.unscaledTime - _savedAt < 2f) return;   // both windows show it
            _savedAt = Time.unscaledTime;
            Tell.Queue("Progress saved.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowTabInstructions))]
        private static void TabHint() { Tell.Queue("You're chatting in two windows. Press Tab to switch between them."); }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ScriptsFinished))]
        private static void ChapterOver() { Tell.Queue("The chapter's chats are over. You can sign out now with the Sign Out button."); }

        // A chosen reply starts being typed; it's done when it's sent.
        [HarmonyPrefix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.WriteText))]
        private static void WritingStarts(ChatManagerBehavior __instance) { ChatState.For(__instance).Writing = true; }

        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.SendChatMessage))]
        private static void Sent(ChatManagerBehavior __instance, ChatMessageType type)
        {
            if (type == ChatMessageType.PlayerMessage) ChatState.For(__instance).Writing = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.HardStop))]
        private static void Stopped(ChatManagerBehavior __instance) { ChatState.For(__instance).Writing = false; }

        // Free typing (a nickname in Chapter 2): letters only, up to 8.
        [HarmonyPrefix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.FreeWriteText))]
        private static void FreeTypingStarts(ChatManagerBehavior __instance, int limit)
        {
            Chat c = ChatState.For(__instance);
            c.FreeTyping = true;
            Tell.Queue("Type your answer with the letter keys, up to " + limit + " letters. Backspace erases. Press Enter to send.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BaseScriptBehavior), nameof(BaseScriptBehavior.DidSubmitFreeText))]
        private static void FreeTypingSent(BaseScriptBehavior __instance)
        {
            if (__instance.cmb == null) return;
            Chat c = ChatState.For(__instance.cmb);
            c.FreeTyping = false;
            c.JustFreeTyped = true;
        }

        // Someone sends a file: Accept / Decline appear under the chat.
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.DisplayAcceptDecline))]
        private static void FileOffer(ChatManagerBehavior __instance)
        {
            ScreenWatch.Later(0.2f, () =>
            {
                if (!__instance.IsShowing) return;
                Navigator.Instance.Refresh();
                foreach (UnityEngine.UI.Button b in __instance.transferWidget.GetComponentsInChildren<UnityEngine.UI.Button>())
                {
                    Navigator.Instance.FocusOn(b.gameObject, true);
                    break;
                }
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowChat1))]
        private static void Chat1(ApplicationManagerBehavior __instance) { ChatState.Switched(__instance.chatManager1); }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowChat2))]
        private static void Chat2(ApplicationManagerBehavior __instance) { ChatState.Switched(__instance.chatManager2); }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowSecretChat))]
        private static void Chat3(ApplicationManagerBehavior __instance) { ChatState.Switched(__instance.chatManager3); }

        // A buddy goes away, comes back or signs off (the buddy list flashes).
        [HarmonyPostfix, HarmonyPatch(typeof(BuddyListManagerBehavior), nameof(BuddyListManagerBehavior.NotifyStatus))]
        private static void Status(string screenname, BuddyListStatus status)
        {
            string s = status == BuddyListStatus.Online ? "is online" : status == BuddyListStatus.Away ? "is away" : "signed off";
            Tell.Queue(screenname + " " + s + ".");
        }
    }
}
