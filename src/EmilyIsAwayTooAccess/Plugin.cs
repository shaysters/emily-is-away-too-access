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
        public const string Id = "shaysters.emilyisawaytoo.access";
        public const string Title = "Emily is Away Too Access";
        public const string Version = "1.0.1";

        internal static ManualLogSource Log;

        // How the version is spoken: "1.0.0" -> "1.0", "0.1.0" -> "0.1", "1.2.3" unchanged.
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
            new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
            gameObject.AddComponent<SettingsMenu>();
            gameObject.AddComponent<ChatWatch>();
            gameObject.AddComponent<ScreenWatch>();
            gameObject.AddComponent<Navigator>();
            gameObject.AddComponent<Keys>();
#if DEV_TOOLS
            if (gameObject.AddComponent<TestDriver>() == null) Log.LogError("TestDriver could not be added.");
#endif
            Log.LogInfo(Title + " " + Version + " loaded.");
            Tell.Queue(Title + " " + SpokenVersion + " loaded.");
        }
    }
}
