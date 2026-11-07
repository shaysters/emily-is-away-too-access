using System.Collections.Generic;
using System.Reflection;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;

namespace EmilyIsAwayTooAccess
{
    // Steam shows achievements as a picture pop-up; the mod says them. Only when the game is
    // connected to Steam (started from Steam), since that's the only time they're earned.
    [HarmonyPatch]
    internal static class Achievements
    {
        private static readonly HashSet<AchievementEnum> Said = new HashSet<AchievementEnum>();

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("SteamManager"), "SetAchievement");
        }

        private static void Postfix(AchievementEnum cheevo)
        {
            if (!Said.Add(cheevo)) return;
            var steam = AccessTools.TypeByName("SteamManager");
            bool online = Traverse.Create(steam).Property("Initialized").GetValue<bool>();
            if (!online) return;
            Tell.Queue("Achievement: " + Name(cheevo) + ".");
        }

        private static string Name(AchievementEnum a)
        {
            switch (a)
            {
                case AchievementEnum.Chapter1: return "finished Chapter 1";
                case AchievementEnum.Chapter2: return "finished Chapter 2";
                case AchievementEnum.Chapter3: return "finished Chapter 3";
                case AchievementEnum.Chapter4: return "finished Chapter 4";
                case AchievementEnum.Chapter5: return "finished Chapter 5";
                case AchievementEnum.IconTopSecret: return "the " + Icons.Name(BuddyIconType.S3TopSecret) + " icon";
                case AchievementEnum.IconRickMorty: return "the " + Icons.Name(BuddyIconType.S3RickMorty) + " icon";
                case AchievementEnum.IconVallhalla: return "the " + Icons.Name(BuddyIconType.S1Vallhalla) + " icon";
                case AchievementEnum.IconHypnospace: return "the " + Icons.Name(BuddyIconType.S1Hypnospace) + " icon";
                case AchievementEnum.IconIdubbbz: return "the " + Icons.Name(BuddyIconType.S2iDubbz) + " icon";
                case AchievementEnum.IconOverwatch: return "the " + Icons.Name(BuddyIconType.S1Overwatch) + " icon";
                case AchievementEnum.IconKappa: return "the " + Icons.Name(BuddyIconType.S3Kappa) + " icon";
                case AchievementEnum.IconKindaFunny: return "the " + Icons.Name(BuddyIconType.S2KindaFunny) + " icon";
                case AchievementEnum.IconGrimecraft: return "the " + Icons.Name(BuddyIconType.S2Grimecraft) + " icon";
                case AchievementEnum.IconNITW: return "the " + Icons.Name(BuddyIconType.S1NITW) + " icon";
                case AchievementEnum.IconNewgrounds: return "the " + Icons.Name(BuddyIconType.S3Newgrounds) + " icon";
                case AchievementEnum.IconTacoma: return "the " + Icons.Name(BuddyIconType.S1Tacoma) + " icon";
                case AchievementEnum.IconHerobrine: return "the " + Icons.Name(BuddyIconType.S4Herobrine) + " icon";
                case AchievementEnum.IconMakeSail: return "the " + Icons.Name(BuddyIconType.S1MakeSail) + " icon";
                case AchievementEnum.IconFNAF: return "the " + Icons.Name(BuddyIconType.S1FNAF) + " icon";
                case AchievementEnum.IconKreygasm: return "the " + Icons.Name(BuddyIconType.S3Kreygasm) + " icon";
                default: return a.ToString();
            }
        }
    }
}
