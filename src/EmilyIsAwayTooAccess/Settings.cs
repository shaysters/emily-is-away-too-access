using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
    // One mod setting: a name and a list of values to step through.
    internal class Setting
    {
        public readonly string Key;
        public readonly string Name;
        public readonly string[] Values;
        public int Index;

        public Setting(string key, string name, int start, params string[] values)
        {
            Key = key; Name = name; Index = start; Values = values;
        }

        public bool On => Index == 1;   // for on/off settings ("off", "on")
        public string Value => Values[Index];
        public void Step(int by) { Index = (Index + by + Values.Length) % Values.Length; }
    }

    internal static class Settings
    {
        private static readonly string[] OffOn = { "off", "on" };

        // Reply timers (Chapters 2 and 3). Index = how many times longer, 0 = no timer.
        public static readonly Setting ReplyTimer = new Setting("ReplyTimer", "Reply timers", 2,
            "off, no time limit", "normal length", "twice as long", "three times as long");
        public static readonly Setting TimerWarnings = new Setting("TimerWarnings", "Warn when a reply timer is running out", 1, OffOn);
        public static readonly Setting OtherWindow = new Setting("OtherWindow", "Read messages in the window you're not looking at", 1, OffOn);
        public static readonly Setting ReadWords = new Setting("ReadWords", "Read each word while typing", 1, OffOn);
        public static readonly Setting ReadErased = new Setting("ReadErased", "Read erased text", 1, OffOn);
        public static readonly Setting ReadReady = new Setting("ReadReady", "Read the finished message before sending", 1, OffOn);
        public static readonly Setting BuddyTyping = new Setting("BuddyTyping", "Read when a buddy is typing", 1, OffOn);
        public static readonly Setting EchoLetters = new Setting("EchoLetters", "Read each letter you type in text boxes and your own answers", 1, OffOn);

        public static readonly List<Setting> All = new List<Setting>
        {
            ReplyTimer, TimerWarnings, OtherWindow, ReadWords, ReadErased, ReadReady, BuddyTyping, EchoLetters,
        };

        // Speech off (test runs only; not a menu option).
        public static bool Muted;

        // BepInEx\config\EmilyIsAwayTooAccess-settings.txt: outside the plugin folder, so
        // reinstalling keeps it, and apart from the game's saves, so the game's reset keeps it.
        private static string File_ => Path.Combine(BepInEx.Paths.ConfigPath, "EmilyIsAwayTooAccess-settings.txt");

        // Multiplier for the game's reply timers; 0 means no timer.
        public static float TimerScale => ReplyTimer.Index;

        public static void Load()
        {
            try
            {
                if (!File.Exists(File_)) return;
                foreach (string line in File.ReadAllLines(File_))
                {
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    Setting s = All.Find(x => x.Key == key);
                    int v;
                    if (s != null && int.TryParse(line.Substring(eq + 1).Trim(), out v) && v >= 0 && v < s.Values.Length) s.Index = v;
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Settings: can't read " + File_ + ": " + e.Message); }
        }

        public static void Save()
        {
            try
            {
                var lines = new List<string>();
                foreach (Setting s in All) lines.Add(s.Key + "=" + s.Index);
                File.WriteAllLines(File_, lines.ToArray());
            }
            catch (Exception e) { Plugin.Log.LogWarning("Settings: can't write " + File_ + ": " + e.Message); }
        }
    }

    // The settings list, opened with M (Ctrl+M while typing). Up/Down move, Left/Right/Enter/Space change the
    // value, Escape or M closes. The game is paused while it's open: time stops (so reply
    // timers wait) and the game's key listeners skip their turn, so keys only go to the list.
    internal class SettingsMenu : MonoBehaviour
    {
        public static bool Open { get; private set; }
        private static int _closedOn = -1;
        private int _at;
        private float _timeScale = 1f;

        // Also true on the frame it closed, so the closing key does nothing else.
        public static bool HoldsKeys => Open || _closedOn == Time.frameCount;

        private void Update()
        {
            try
            {
                if (!Open)
                {
                    if (Input.GetKeyDown(KeyCode.M) && (Keys.Ctrl() || !Keys.Typing()) && !HoldsKeys) { Keys.UseKey(); Show(); }
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M)) Close();
                else if (Input.GetKeyDown(KeyCode.DownArrow)) Move(1);
                else if (Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
                else if (Input.GetKeyDown(KeyCode.Home)) { _at = 0; Read(); }
                else if (Input.GetKeyDown(KeyCode.End)) { _at = Settings.All.Count; Read(); }
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) Change(-1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return)
                         || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Change(1);
            }
            catch (Exception e) { Plugin.Log.LogError("SettingsMenu: " + e); }
        }

        public void Show()
        {
            Open = true;
            _at = 0;
            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
            Tell.Now("Mod settings, game paused. " + Describe(0));
        }

        public void Close()
        {
            Open = false;
            _closedOn = Time.frameCount;
            Time.timeScale = _timeScale;
            Settings.Save();
            Tell.Now("Settings closed.");
        }

        private void Move(int by)
        {
            int n = Settings.All.Count + 1;   // the last item is "Close"
            _at = (_at + by + n) % n;
            Read();
        }

        public void Change(int by)
        {
            if (_at >= Settings.All.Count) { Close(); return; }
            Setting s = Settings.All[_at];
            s.Step(by);
            Settings.Save();
            Tell.Now(s.Value);
        }

        private void Read() { Tell.Now(Describe(_at)); }

        private static string Describe(int i)
        {
            if (i >= Settings.All.Count) return "Close";
            Setting s = Settings.All[i];
            return s.Name + ", " + s.Value;
        }

        // For the test driver.
        public void Pick(int i) { _at = Mathf.Clamp(i, 0, Settings.All.Count); Read(); }
    }

    // While the settings list is open, the game's key-reading coroutines skip their turn:
    // typing a reply, the 1-3 choice keys, free typing, sign-in and the profile editor keys.
    [HarmonyPatch]
    internal static class HoldGameKeys
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.WriteText)));
            yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.FreeWriteText)));
            yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(TextButtonsBehavior), nameof(TextButtonsBehavior.KeyboardListener)));
            yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(SignInManagerBehavior), "KeyboardListener"));
            MethodInfo info = AccessTools.Method(typeof(BuddyInfoManagerBehavior), "KeyboardListener");
            if (info != null) yield return AccessTools.EnumeratorMoveNext(info);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!SettingsMenu.HoldsKeys && !Keys.ModUsedKeyThisFrame && !Keys.CtrlDownAlone()) return true;
            __result = true;   // still running; it tries again next frame
            return false;
        }
    }

    // Tab switches between the two chat windows, but in the game only once one of them
    // has been clicked open. With the mod, Tab also opens a chat when none is open yet, or
    // from a secret buddy's chat: the one waiting for a reply, or else the first.
    // Not while the settings list is open.
    [HarmonyPatch(typeof(BuddyListManagerBehavior), nameof(BuddyListManagerBehavior.Update))]
    internal static class TabKey
    {
        private static bool Prefix(BuddyListManagerBehavior __instance)
        {
            if (SettingsMenu.HoldsKeys) return false;
            if (!Input.GetKeyDown(KeyCode.Tab) || Keys.TextEntryActive()) return true;
            var rows = __instance.buddyRows;
            if (rows.Count < 2 || rows[0].IsFocused || rows[1].IsFocused || rows[0].IsHidden) return true;
            ApplicationManagerBehavior app = ApplicationManagerBehavior.Instance;
            Chat on = ChatState.Showing();
            bool inSecret = on != null && app != null && on.Window == app.chatManager3;   // a secret buddy's chat
            if ((on != null && !inSecret) || BaseMasterScriptBehavior.Instance == null) return true;
            int pick = 0;
            if (app != null && ChatState.For(app.chatManager2).ChoicesUp && !ChatState.For(app.chatManager1).ChoicesUp) pick = 1;
            rows[pick].DidPressRow();
            return false;
        }
    }
}
