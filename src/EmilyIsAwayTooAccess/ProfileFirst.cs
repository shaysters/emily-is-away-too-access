using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // At the start of a chapter the game starts the chats as soon as you answer the first
    // profile question, so messages talk over the rest of the questions. The mod holds the
    // chats until the profile is finished (or until you leave the profile window), then
    // starts them. Nothing in the story changes; they just begin a few seconds later.
    [HarmonyPatch]
    internal static class ProfileFirst
    {
        private static bool _held;

        // True while the profile questions are on screen.
        private static bool AnsweringProfile()
        {
            BuddyInfoManagerBehavior b = Object.FindObjectOfType<BuddyInfoManagerBehavior>();
            if (b == null || b.buddyWindow.alpha < 0.5f) return false;
            return b.profile1Buttons.alpha > 0.5f || b.profile2Buttons.alpha > 0.5f || b.profile3Buttons.alpha > 0.5f;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BaseMasterScriptBehavior), nameof(BaseMasterScriptBehavior.PlayScripts))]
        private static bool Hold()
        {
            if (Traverse.Create(BaseMasterScriptBehavior.Instance).Field("hasPlayedScripts").GetValue<bool>()) return true;
            if (!AnsweringProfile()) return true;
            _held = true;
            Plugin.Log.LogInfo("Chats held until the profile is done.");
            return false;
        }

        public static void ProfileFinished() { Release(); }

        // Leaving the profile window (opening a chat, the buddy list...) starts them too.
        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.HideBuddyInfo))]
        private static void ProfileClosed() { Release(); }

        private static void Release()
        {
            if (!_held) return;
            _held = false;
            BaseMasterScriptBehavior m = BaseMasterScriptBehavior.Instance;
            if (m != null) m.PlayScripts();
        }
    }
}
