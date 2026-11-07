using System.IO;
using BepInEx;
using BepInEx.Logging;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;

namespace EmilyIsAwayTooAccess
{
    [BepInPlugin(Id, Title, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Id = "shaysters.emilyonline.access";
        public const string Title = "Emily Online Access";
        public const string Version = "1.0.3";

        internal static ManualLogSource Log;

        public static string SpokenVersion
        {
            get
            {
                string[] parts = Version.Split('.');
                return parts.Length == 3 && parts[2] == "0" ? parts[0] + "." + parts[1] : Version;
            }
        }

        private void Awake()
        {
            Log = Logger;
            PrismOutput.Start(Path.GetDirectoryName(typeof(Plugin).Assembly.Location));
            Settings.Load();
            Tell.SpriteName = Emotes.Name;
            new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
            gameObject.AddComponent<SettingsMenu>();
            gameObject.AddComponent<ScreenWatch>();
            gameObject.AddComponent<Navigator>();
            gameObject.AddComponent<Keys>();
#if DEV_TOOLS
            gameObject.AddComponent<OnlineTestDriver>();
#endif
            Log.LogInfo(Title + " " + Version + " loaded.");
            Tell.Queue(Title + " " + SpokenVersion + " loaded.");
        }
    }
}
