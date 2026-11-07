using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // Descriptions of the game's pictures that aren't buddy icons, written by looking at
    // each one: the installer's desktop backgrounds, the banner ad by the icon picker, the
    // pictures at the top of each chapter page, and the art project Emily sends in Chapter 4.
    internal static class Pictures
    {
        // The installer's three backgrounds; the game also saves the chosen one to the desktop.
        public static string Desktop(int choice)
        {
            switch (choice)
            {
                case 1: return "a big blue mountain at night under a lavender sky, with a bright full moon";
                case 2: return "a rolling green hill under a bright blue sky with white clouds";
                case 3: return "a calm turquoise sea with a small sandy island of three palm trees, and a white sailboat on the horizon";
                default: return "";
            }
        }

        // The picture across the top of a chapter's page in the menu.
        public static string ChapterHeader(int chapter)
        {
            switch (chapter)
            {
                case 1: return "a green hill under a blue summer sky";
                case 2: return "a path through trees with orange autumn leaves";
                case 3: return "the moon over a blue mountain at night";
                case 4: return "Stonehenge under a blue sky";
                case 5: return "a sailboat and a little palm-tree island on the sea";
                default: return "";
            }
        }

        // The banner ad that plays beside the sign-in window and the icon picker; the game
        // picks it by chapter. Each ad is four pictures that fade from one to the next.
        public static string BannerAd(int level)
        {
            switch (level)
            {
                case 2: return "a tan sheepskin boot that turns to show its fluffy cuff, ending on the UGH boots logo";
                case 3: return "black silhouettes dancing with white earphones on bright blue, pink and purple, ending on a white pear logo on lime green, a music player ad";
                case 4: return "two people watching a television that says Loading, with the words Streaming slow? End streaming. Blockblister by mail, and a blue and yellow Blockblister card";
                case 5: return "the words Finish the fight appearing one by one in light blue, ending on a glowing blue ring emblem";
                default: return "a white phone whose screen swings out to show a keyboard, ending on the words Sideflick 3";
            }
        }

        // The art project Emily sends you in Chapter 4, depending on the idea you picked.
        public static string ArtProject(int choice)
        {
            switch (choice)
            {
                case 2: return "black palm tree silhouettes against a pink, orange and blue sunset";
                case 3: return "a green monster's face on pink, with one yellow eye, one eye crossed out with an X, and a toothy grin";
                default: return "a big blocky drawing of an instant messenger window, with red and blue chat lines, in front of a green hill and blue sky";
            }
        }
    }

    [HarmonyPatch]
    internal static class PicturePatches
    {
        // Say the banner ad once per chapter start, after the screen itself is read.
        private static float _adSaid = -100f;

        [HarmonyPostfix, HarmonyPatch(typeof(BannerManagerBehavior), nameof(BannerManagerBehavior.Show))]
        private static void Banner()
        {
            if (Time.unscaledTime - _adSaid < 60f) return;
            _adSaid = Time.unscaledTime;
            int level = ChatValues.GetCurrentLevel();
            ScreenWatch.Later(0.5f, () => Tell.Queue("Beside the window, a banner ad: " + Pictures.BannerAd(level) + "."));
        }

        // Chapter 4: Emily's art project arrives on the desktop.
        [HarmonyPostfix, HarmonyPatch(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.SendChatMessage))]
        private static void ArtProject(string text, ChatMessageType type)
        {
            if ((type == ChatMessageType.OtherFileTransferred || type == ChatMessageType.OtherFileSent) && text == "artproject.png")
                Tell.Queue("The picture shows " + Pictures.ArtProject(ChatValues.Ch4PlayerProjectChoice) + ".");
        }

        // Profiles show the buddy's icon; remember which, for the profile readout.
        public static BuddyIconType ProfileIcon = BuddyIconType.None;

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.SetUpOtherInfo))]
        private static void OtherProfile(BuddyIconType icon, Sprite avatar) { ProfileIcon = avatar != null ? BuddyIconType.None : icon; }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.SetUpPlayerInfo))]
        private static void OwnProfile() { ProfileIcon = ChatValues.GetPlayerBuddyIcon(); }
    }
}
