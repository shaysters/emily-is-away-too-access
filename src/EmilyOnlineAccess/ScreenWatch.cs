using System;
using System.Collections.Generic;
using System.Text;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EmilyIsAwayTooAccess
{
    // Announces Emily Online's screens and pop-ups, and runs small delayed jobs.
    internal class ScreenWatch : MonoBehaviour
    {
        private static ScreenWatch _me;
        private readonly List<KeyValuePair<float, Action>> _later = new List<KeyValuePair<float, Action>>();
        private string _scene = "";
        private readonly List<TMP_Text> _live = new List<TMP_Text>();
        private readonly Dictionary<TMP_Text, string> _liveSaid = new Dictionary<TMP_Text, string>();
        private bool _liveFound;

        private void Awake() { _me = this; }

        public static void Later(float seconds, Action job)
        {
            if (_me != null) _me._later.Add(new KeyValuePair<float, Action>(Time.unscaledTime + seconds, job));
        }

        private void Update()
        {
            try
            {
                string scene = SceneManager.GetActiveScene().name;
                if (scene != _scene)
                {
                    _scene = scene;
                    Plugin.Log.LogInfo("Scene: " + scene);
                    Navigator.Instance?.Forget();
                    _live.Clear();
                    _liveFound = false;
                }
                WatchLive();
                for (int i = _later.Count - 1; i >= 0; i--)
                {
                    if (Time.unscaledTime < _later[i].Key) continue;
                    Action job = _later[i].Value;
                    _later.RemoveAt(i);
                    try { job(); } catch (Exception e) { Plugin.Log.LogError("Later: " + e); }
                }
            }
            catch (Exception e) { Plugin.Log.LogError("ScreenWatch: " + e); }
        }

        // Labels that change by themselves: signing out, and your warning level.
        private void WatchLive()
        {
            if (!_liveFound)
            {
                _liveFound = true;
                ApplicationManagerBehavior app = FindObjectOfType<ApplicationManagerBehavior>();
                if (app != null)
                {
                    _live.Add(app.connectionLabel);
                    if (app.chatManager != null) _live.Add(app.chatManager.rememberLabel);
                }
            }
            foreach (TMP_Text t in _live)
            {
                if (t == null) continue;
                string now = Ui.Shown(t.transform) ? Ui.Text(t).TrimEnd('.') : "";
                string old;
                _liveSaid.TryGetValue(t, out old);
                if (now == (old ?? "")) continue;
                _liveSaid[t] = now;
                if (now.Length > 0) Tell.Queue(Tell.Stop(now));
            }
        }

        public static void ReadPanel(Transform root, string title, bool focus = true)
        {
            var sb = new StringBuilder(title ?? "");
            foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>())
            {
                if (!Ui.Shown(t.transform) || t.GetComponentInParent<Selectable>() != null) continue;
                if (t.GetComponent<TextLinkButton>() != null || t.name.Contains("Input")) continue;
                string s = Ui.Text(t);
                if (s.Length == 0) continue;
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(Tell.Stop(s));
            }
            Tell.Now(sb.ToString());
            if (!focus) return;
            Later(0.05f, () =>
            {
                Navigator nav = Navigator.Instance;
                foreach (Control c in nav.Refresh())
                    if (c.Target.transform.IsChildOf(root)) { nav.FocusOn(c.Go, true); return; }
                nav.FocusFirst(true);
            });
        }

        public static Transform ModalRoot()
        {
            AlertPopupBehavior alert = AlertPopupBehavior.Instance;
            if (alert != null && Ui.Shown(alert.alertPopup.transform) && alert.alertPopup.blocksRaycasts) return alert.alertPopup.transform;
            return null;
        }
    }

    [HarmonyPatch]
    internal static class OnlineScreenPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AlertPopupBehavior), nameof(AlertPopupBehavior.Show))]
        private static void Alert(AlertPopupBehavior __instance)
        {
            string text = Ui.Text(__instance.alertLabel);
            Tell.Now(text.Length > 0 ? text : "Emily Online can't start: it needs Steam, and Steam chat must be allowed on your account.");
            ScreenWatch.Later(0.1f, () =>
            {
                Transform cancel = null;
                foreach (Button b in __instance.alertPopup.GetComponentsInChildren<Button>())
                    if (b.name.ToLowerInvariant().Contains("cancel")) cancel = b.transform;
                if (cancel != null) Navigator.Instance.FocusOn(cancel.gameObject, true);
                else Navigator.Instance.FocusFirst(true);
            });
        }

        // Signed in: the welcome text and the buddy list.
        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.Start))]
        private static void Started(ApplicationManagerBehavior __instance)
        {
            ScreenWatch.Later(1f, () =>
            {
                int total = 0, on = 0;
                foreach (BuddyListRowBehavior r in UnityEngine.Object.FindObjectsOfType<BuddyListRowBehavior>())
                {
                    FriendStruct f = Traverse.Create(r).Field("buddyData").GetValue<FriendStruct>();
                    if (f == null) continue;
                    total++;
                    if (f.state != FriendState.Offline) on++;
                }
                Tell.Now("Emily Online. " + Tell.Stop(Ui.Text(__instance.welcomeLabel)) + " Your buddy list has " + total + " Steam friends, " + on + " online. Use Up and Down to choose a buddy, and Enter to chat.");
                Navigator.Instance.FocusFirst(false);
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowSelfBuddyInfo))]
        private static void OwnProfile(ApplicationManagerBehavior __instance) { Profile(__instance.buddyInfoManager); }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowOtherBuddyInfo))]
        private static void OtherProfile(ApplicationManagerBehavior __instance) { Profile(__instance.buddyInfoManager); }

        private static void Profile(BuddyInfoManagerBehavior b)
        {
            string status = Ui.Text(b.statusLabel);
            Tell.Now("Profile: " + Ui.Text(b.usernameLabel) + (status.Length > 0 ? ", " + status : "") + ". " + Ui.Text(b.infoLabel));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.ShowEditProfile))]
        private static void EditProfile(BuddyInfoManagerBehavior __instance)
        {
            ScreenWatch.Later(0.2f, () => ScreenWatch.ReadPanel(__instance.infoSettings.settingsCanvas.transform, "Edit profile."));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.DidPressEditProfileButton))]
        private static void ProfileButton(BuddyInfoManagerBehavior __instance)
        {
            if (Ui.Text(__instance.editLabel) == "edit profile") Tell.Now("Profile saved. " + Ui.Text(__instance.infoLabel));
        }

        // Paging through the ready-made profiles.
        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoSettingsBehavior), nameof(BuddyInfoSettingsBehavior.DidPressLeftButton))]
        private static void ProfilesLeft(BuddyInfoSettingsBehavior __instance) { ScreenWatch.ReadPanel(__instance.settingsCanvas.transform, "", false); }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoSettingsBehavior), nameof(BuddyInfoSettingsBehavior.DidPressRightButton))]
        private static void ProfilesRight(BuddyInfoSettingsBehavior __instance) { ScreenWatch.ReadPanel(__instance.settingsCanvas.transform, "", false); }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoSettingsBehavior), nameof(BuddyInfoSettingsBehavior.DidPressPreviewButton))]
        private static void ProfilePicked(BuddyInfoSettingsBehavior __instance)
        {
            BuddyInfoManagerBehavior b = __instance.manager;
            Tell.Now("Profile preview: " + Ui.Text(b.infoLabel) + ". Choose save profile to keep it.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowSettings))]
        private static void GameSettings(ApplicationManagerBehavior __instance)
        {
            ScreenWatch.ReadPanel(__instance.settingsManager.transform, "Settings.");
        }

        // Colour pickers close on a mouse release in the game; with the keyboard, on a pick.
        [HarmonyPostfix, HarmonyPatch(typeof(ColorPickerBehavior), nameof(ColorPickerBehavior.Show))]
        private static void ColourPicker(ColorPickerBehavior __instance)
        {
            ColorSwatchBehavior first = __instance.GetComponentInChildren<ColorSwatchBehavior>();
            Tell.Now(first != null ? "Choose a colour." : "Choose an emote. It goes into your message.");
            ScreenWatch.Later(0.1f, () =>
            {
                Selectable sel = first != null ? first.GetComponentInChildren<Selectable>() : __instance.GetComponentInChildren<Selectable>();
                if (sel != null) Navigator.Instance.FocusOn(sel.gameObject, true);
            });
        }

        // The emote picker (the same kind of picker): close it after a pick; the message box
        // gets the keyboard back and reads the emote that went in.
        [HarmonyPostfix, HarmonyPatch(typeof(ColorPickerBehavior), nameof(ColorPickerBehavior.DidPressEmoji))]
        private static void EmotePicked(ColorPickerBehavior __instance, int index)
        {
            __instance.Hide();
            Tell.Now("Added " + Emotes.Name(index) + " to your message.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ColorSwatchBehavior), nameof(ColorSwatchBehavior.DidPressSwatch))]
        private static void ColourPicked(ColorSwatchBehavior __instance)
        {
            ColorPickerBehavior p = __instance.GetComponentInParent<ColorPickerBehavior>();
            if (p != null) p.Hide();
            Tell.Now("Colour set to " + Labels.ColourName(__instance.color) + ".");
        }
    }
}
