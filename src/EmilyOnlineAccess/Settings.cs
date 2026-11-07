using System;
using System.Collections.Generic;
using System.IO;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using UnityEngine;

namespace EmilyIsAwayTooAccess
{
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

        public bool On => Index == 1;
        public string Value => Values[Index];
        public void Step(int by) { Index = (Index + by + Values.Length) % Values.Length; }
    }

    internal static class Settings
    {
        private static readonly string[] OffOn = { "off", "on" };

        public static readonly Setting OtherChats = new Setting("OtherChats", "Read messages from buddies whose chat isn't open", 1, OffOn);
        public static readonly Setting Statuses = new Setting("Statuses", "Read status changes", 1,
            "off", "only the buddy you're chatting with", "every buddy");
        public static readonly Setting BuddyTyping = new Setting("BuddyTyping", "Read when a buddy is typing", 1, OffOn);
        public static readonly Setting OwnSent = new Setting("OwnSent", "Read your own messages when sent", 0, OffOn);
        public static readonly Setting EchoLetters = new Setting("EchoLetters", "Read each letter as you type", 1, OffOn);

        public static readonly List<Setting> All = new List<Setting> { EchoLetters, OtherChats, Statuses, BuddyTyping, OwnSent };

        public static bool Muted;

        private static string File_ => Path.Combine(BepInEx.Paths.ConfigPath, "EmilyOnlineAccess-settings.txt");

        public static void Load()
        {
            try
            {
                if (!File.Exists(File_)) return;
                foreach (string line in File.ReadAllLines(File_))
                {
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    Setting s = All.Find(x => x.Key == line.Substring(0, eq).Trim());
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

    // The settings list: M (outside text boxes) or Ctrl+M (anywhere). Up/Down move,
    // Left/Right/Enter/Space change, Escape or M closes. While it's open, keys don't reach
    // the chat box.
    internal class SettingsMenu : MonoBehaviour
    {
        public static bool Open { get; private set; }
        private static int _closedOn = -1;
        private int _at;
        private UnityEngine.UI.Selectable _box;

        public static bool HoldsKeys => Open || _closedOn == Time.frameCount;

        private void Update()
        {
            try
            {
                bool m = Input.GetKeyDown(KeyCode.M);
                if (!Open)
                {
                    if (m && (Keys.Ctrl() || !Keys.TextEntryActive()) && !HoldsKeys) Show();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Escape) || m) Close();
                else if (Input.GetKeyDown(KeyCode.DownArrow)) Move(1);
                else if (Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
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
            // Take the keyboard away from the chat box while the list is open.
            _box = Navigator.FocusedBox();
            if (_box != null) Ui.DeactivateBox(_box);
            Tell.Now("Mod settings. " + Describe(0));
        }

        public void Close()
        {
            Open = false;
            _closedOn = Time.frameCount;
            Settings.Save();
            if (_box != null) { Ui.ActivateBox(_box); _box = null; }
            Tell.Now("Settings closed.");
        }

        private void Move(int by)
        {
            int n = Settings.All.Count + 1;
            _at = (_at + by + n) % n;
            Tell.Now(Describe(_at));
        }

        public void Change(int by)
        {
            if (_at >= Settings.All.Count) { Close(); return; }
            Setting s = Settings.All[_at];
            s.Step(by);
            Settings.Save();
            Tell.Now(s.Value);
        }

        private static string Describe(int i)
        {
            if (i >= Settings.All.Count) return "Close";
            return Settings.All[i].Name + ", " + Settings.All[i].Value;
        }
    }

    // The chat's own key listener (Enter sends) skips its turn while the list is open.
    [HarmonyPatch]
    internal static class HoldGameKeys
    {
        private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ChatManagerBehavior), nameof(ChatManagerBehavior.WriteText)));
        }

        private static bool Prefix(ref bool __result)
        {
            if (!SettingsMenu.HoldsKeys && !Keys.ModUsedKeyThisFrame) return true;
            __result = true;
            return false;
        }
    }
}
