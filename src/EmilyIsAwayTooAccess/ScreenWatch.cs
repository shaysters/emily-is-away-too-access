using System;
using System.Collections.Generic;
using EmilyIsAwayTooAccess.Speech;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EmilyIsAwayTooAccess
{
    // Announces screens and pop-ups as they appear, and runs small delayed jobs.
    internal class ScreenWatch : MonoBehaviour
    {
        private static ScreenWatch _me;
        private readonly List<KeyValuePair<float, Action>> _later = new List<KeyValuePair<float, Action>>();
        private string _scene = "";

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

        // Labels that count up or step through messages by themselves
        // ("Connecting." "Connecting.." ...): read whenever the words change.
        private readonly List<TMP_Text> _live = new List<TMP_Text>();
        private readonly Dictionary<TMP_Text, string> _liveSaid = new Dictionary<TMP_Text, string>();
        private bool _liveFound;

        private void WatchLive()
        {
            if (!_liveFound)
            {
                _liveFound = true;
                InstallerInstallBehavior inst = FindObjectOfType<InstallerInstallBehavior>();
                if (inst != null) _live.Add(inst.installText);
                MainMenuManagerBehavior menu = FindObjectOfType<MainMenuManagerBehavior>();
                if (menu != null) _live.Add(menu.connectionLabel);
                ApplicationManagerBehavior app = FindObjectOfType<ApplicationManagerBehavior>();
                if (app != null) _live.Add(app.connectionLabel);
                CodePopupBehavior code = FindObjectOfType<CodePopupBehavior>();
                if (code != null) { _live.Add(code.outputLabel); _live.Add(code.multipleUnlockLabel); }
                ExportPopupBehavior export = FindObjectOfType<ExportPopupBehavior>();
                if (export != null) _live.Add(export.exportLabel);
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

        // Reads the text of a window or page (not its buttons), then puts the navigator
        // on its first control.
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

        // The open pop-up, if any: the navigator stays inside it.
        public static Transform ModalRoot()
        {
            AlertPopupBehavior alert = AlertPopupBehavior.Instance;
            if (alert != null && Ui.Shown(alert.alertPopup.transform) && alert.alertPopup.blocksRaycasts) return alert.alertPopup.transform;
            CodePopupBehavior code = CodePopupBehavior.Instance;
            if (code != null && Ui.Shown(code.codePopup.transform) && code.codePopup.blocksRaycasts) return code.codePopup.transform;
            ExportPopupBehavior export = ExportPopupBehavior.Instance;
            if (export != null && Ui.Shown(export.codePopup.transform) && export.codePopup.blocksRaycasts) return export.codePopup.transform;
            return null;
        }

        public static string WhereText()
        {
            return "Screen " + SceneManager.GetActiveScene().name;
        }
    }

    [HarmonyPatch]
    internal static class ScreenPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(AlertPopupBehavior), nameof(AlertPopupBehavior.Show))]
        private static void Alert(AlertPopupBehavior __instance)
        {
            Tell.Now(Ui.Text(__instance.alertLabel));
            // Start on Cancel, so pressing Enter by accident can't lose progress or quit.
            ScreenWatch.Later(0.1f, () =>
            {
                Transform cancel = null;
                foreach (UnityEngine.UI.Button b in __instance.alertPopup.GetComponentsInChildren<UnityEngine.UI.Button>())
                    if (b.name.ToLowerInvariant().Contains("cancel")) cancel = b.transform;
                if (cancel != null) Navigator.Instance.FocusOn(cancel.gameObject, true);
                else Navigator.Instance.FocusFirst(true);
            });
        }

        // The fake installer that runs the first time the game starts.
        [HarmonyPostfix, HarmonyPatch(typeof(InstallerManagerBehavior), nameof(InstallerManagerBehavior.Start))]
        private static void InstallIntro(InstallerManagerBehavior __instance) { ScreenWatch.Later(0.5f, () => ScreenWatch.ReadPanel(__instance.introCanvas.transform, "")); }

        [HarmonyPostfix, HarmonyPatch(typeof(InstallerManagerBehavior), nameof(InstallerManagerBehavior.DidPressIntroNext))]
        private static void InstallDesktop(InstallerManagerBehavior __instance) { ScreenWatch.ReadPanel(__instance.desktopCanvas.transform, ""); }

        [HarmonyPostfix, HarmonyPatch(typeof(InstallerManagerBehavior), nameof(InstallerManagerBehavior.ContinueFromDesktop))]
        private static void Installing(InstallerManagerBehavior __instance) { Tell.Now("Installing."); }

        [HarmonyPostfix, HarmonyPatch(typeof(InstallerManagerBehavior), nameof(InstallerManagerBehavior.DidPressInstallNext))]
        private static void InstallDone(InstallerManagerBehavior __instance)
        {
            ScreenWatch.ReadPanel(__instance.finishCanvas.transform, "");
            string pic = Pictures.Desktop(__instance.DesktopChoice);
            if (pic.Length > 0) Tell.Queue("The picture: " + pic + ".");
        }

        // Main menu pages.
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuManagerBehavior), nameof(MainMenuManagerBehavior.ShowTitleMenu))]
        private static void TitleMenu(MainMenuManagerBehavior __instance) { ScreenWatch.ReadPanel(__instance.playMenu.transform, "Main menu."); }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuManagerBehavior), nameof(MainMenuManagerBehavior.ShowSettingsMenu))]
        private static void SettingsPage(MainMenuManagerBehavior __instance) { ScreenWatch.ReadPanel(__instance.settingsMenu.transform, "Settings.", true); }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuManagerBehavior), nameof(MainMenuManagerBehavior.ShowGalleryMenu))]
        private static void GalleryPage(MainMenuManagerBehavior __instance) { ScreenWatch.ReadPanel(__instance.galleryMenu.transform, "Icon gallery."); }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuManagerBehavior), nameof(MainMenuManagerBehavior.ShowChapterMenu))]
        private static void ChapterPage(MainMenuManagerBehavior __instance, int chapter)
        {
            string pic = Pictures.ChapterHeader(chapter);
            ScreenWatch.ReadPanel(__instance.chapterMenu.menuWidget.transform, "Chapter " + chapter + "." + (pic.Length > 0 ? " Picture: " + pic + "." : ""));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SignInManagerBehavior), nameof(SignInManagerBehavior.ShowSignIn))]
        private static void SignIn(SignInManagerBehavior __instance)
        {
            Tell.Now("Sign in to EOL Instant Messenger. Type a screen name and your first name, or use Randomize.");
            ScreenWatch.Later(0.1f, () => Navigator.Instance.FocusOn(__instance.screennameInput.gameObject, true));
        }

        // The ending: a title card, then pages (your summary, statistics, an unlocked icon,
        // thanks) with Back and Next.
        [HarmonyPostfix, HarmonyPatch(typeof(OutroManagerBehavior), nameof(OutroManagerBehavior.Start))]
        private static void OutroStarts(OutroManagerBehavior __instance)
        {
            ScreenWatch.Later(1f, () => ScreenWatch.ReadPanel(__instance.titleCanvas.transform, "", false));
        }

        private static BuddyIconType _outroIcon = BuddyIconType.None;

        [HarmonyPostfix, HarmonyPatch(typeof(OutroIconUnlockBehavior), nameof(OutroIconUnlockBehavior.SetUp))]
        private static void OutroIcon(BuddyIconType unlocked) { _outroIcon = unlocked; }

        [HarmonyPrefix, HarmonyPatch(typeof(OutroSpecialThanksBehavior), nameof(OutroSpecialThanksBehavior.Show))]
        private static void ThanksPage() { Tell.Now("Special thanks, my buddy list."); }

        // The thanks page types out a buddy list of people the developer thanks, one by one.
        [HarmonyPrefix, HarmonyPatch(typeof(OutroBuddyWindowBehavior), nameof(OutroBuddyWindowBehavior.AddBuddyName))]
        private static void ThanksName(string username, string realname)
        {
            Tell.Queue(Tell.Stop(Tell.Plain(realname)) + " " + Tell.Plain(username));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OutroManagerBehavior), "DisplayCurrentPage")]
        private static void OutroPage(OutroManagerBehavior __instance)
        {
            ScreenWatch.Later(0.6f, () =>
            {
                foreach (Component page in new Component[] { __instance.playerCanvas, __instance.statsPanel, __instance.iconPanel, __instance.thanksPanel })
                    if (page != null && Ui.Shown(page.transform))
                    {
                        string title = page == __instance.iconPanel && _outroIcon != BuddyIconType.None ? "New icon: " + Icons.Describe(_outroIcon) + ". " : "";
                        if (page == __instance.thanksPanel) { Navigator.Instance.FocusFirst(false); return; }   // its names are read as they appear
                        ScreenWatch.ReadPanel(page.transform, title);
                        return;
                    }
            });
        }

        // Colour pickers for your chat text and profile. The game closes them when the mouse
        // is released anywhere; with the keyboard they close once a colour is picked.
        [HarmonyPostfix, HarmonyPatch(typeof(ColorPickerBehavior), nameof(ColorPickerBehavior.Show))]
        private static void ColourPicker(ColorPickerBehavior __instance)
        {
            string what = __instance.pickerType == ColorPickerType.TextFontColor || __instance.pickerType == ColorPickerType.InfoFontColor ? "text" : "background";
            Tell.Now("Choose a " + what + " colour.");
            ScreenWatch.Later(0.1f, () =>
            {
                ColorSwatchBehavior first = __instance.GetComponentInChildren<ColorSwatchBehavior>();
                if (first == null) return;
                UnityEngine.UI.Selectable sel = first.GetComponentInChildren<UnityEngine.UI.Selectable>();
                Navigator.Instance.FocusOn(sel != null ? sel.gameObject : first.gameObject, true);
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ColorPickerBehavior), nameof(ColorPickerBehavior.DidPressColorSwatch))]
        private static void ColourPicked(ColorPickerBehavior __instance, Color c)
        {
            __instance.Hide();
            Tell.Now("Colour set to " + Labels.ColourName(c) + ".");
        }

        // Export Log on a finished chapter: saves the chats as web pages on the desktop.
        [HarmonyPostfix, HarmonyPatch(typeof(ExportPopupBehavior), nameof(ExportPopupBehavior.ShowExport))]
        private static void Export(ExportPopupBehavior __instance)
        {
            ScreenWatch.Later(3.6f, () => { if (Ui.Shown(__instance.codePopup.transform)) Navigator.Instance.FocusFirst(true); });
        }

        // Icon gallery (main menu).
        private static void GalleryPageText(IconGalleryManagerBehavior g)
        {
            var tr = Traverse.Create(g);
            string text = "";
            foreach (string f in new[] { "headerText", "captionText", "hintText" })
            {
                TMP_Text t = tr.Field(f).GetValue<TMP_Text>();
                if (t != null && Ui.Text(t).Length > 0) text += Tell.Stop(Ui.Text(t)) + " ";
            }
            Tell.Now(text.Trim());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(IconGalleryManagerBehavior), nameof(IconGalleryManagerBehavior.DidPressLeftButton))]
        private static void GalleryLeft(IconGalleryManagerBehavior __instance) { GalleryPageText(__instance); }

        [HarmonyPostfix, HarmonyPatch(typeof(IconGalleryManagerBehavior), nameof(IconGalleryManagerBehavior.DidPressRightButton))]
        private static void GalleryRight(IconGalleryManagerBehavior __instance) { GalleryPageText(__instance); }

        [HarmonyPostfix, HarmonyPatch(typeof(IconGalleryManagerBehavior), nameof(IconGalleryManagerBehavior.SetBuddyIcon))]
        private static void GalleryPicked(BuddyIconType type) { Tell.Now("Icon set to " + Icons.Name(type) + "."); }

        // Unlocking with a code in the gallery's Enter Code window.
        [HarmonyPostfix, HarmonyPatch(typeof(IconGalleryManagerBehavior), nameof(IconGalleryManagerBehavior.FlipToIcon))]
        private static void CodeUnlockedIcon(BuddyIconType type)
        {
            CodePopupBehavior p = CodePopupBehavior.Instance;
            if (p != null && Ui.Shown(p.codePopup.transform)) Tell.Queue("Unlocked " + Icons.Describe(type) + ".");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CodePopupBehavior), nameof(CodePopupBehavior.Show))]
        private static void CodeWindow(CodePopupBehavior __instance)
        {
            Tell.Now("Enter code. Type a code and press Enter.");
            ScreenWatch.Later(0.1f, () => Navigator.Instance.FocusOn(__instance.codenameInput.gameObject, true));
        }

        // An icon unlocked during the game (secret screen names, links, endings).
        [HarmonyPostfix, HarmonyPatch(typeof(CodePopupBehavior), nameof(CodePopupBehavior.ShowUnlocked))]
        private static void IconUnlocked(CodePopupBehavior __instance, BuddyIconType unlocked)
        {
            Tell.Queue("New buddy icon unlocked: " + Icons.Describe(unlocked) + ".");
            ScreenWatch.Later(0.1f, () => Navigator.Instance.FocusFirst(true));
        }

        // Buddy icon picker at the start of each chapter.
        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowInitialLoad))]
        private static void IconPicker(ApplicationManagerBehavior __instance)
        {
            BuddyIconSettingsBehavior icons = __instance.buddyInfoManager.buddyIcons;
            Tell.Now("Choose your buddy icon. " + Tell.Stop(Ui.Text(icons.titleLabel)) + " Your icon now: " + Icons.Name(ChatValues.GetPlayerBuddyIcon()) + ".");
            ScreenWatch.Later(0.1f, () =>
            {
                foreach (BuddyIconButtonBehavior b in icons.buddyIcons)
                    if (Ui.Shown(b.transform) && b.buddyButton.interactable) { Navigator.Instance.FocusOn(b.buddyButton.gameObject, true); return; }
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyIconSettingsBehavior), nameof(BuddyIconSettingsBehavior.DidPressLeftButton))]
        private static void IconPageLeft(BuddyIconSettingsBehavior __instance) { Tell.Now(Tell.Stop(Ui.Text(__instance.titleLabel))); }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyIconSettingsBehavior), nameof(BuddyIconSettingsBehavior.DidPressRightButton))]
        private static void IconPageRight(BuddyIconSettingsBehavior __instance) { Tell.Now(Tell.Stop(Ui.Text(__instance.titleLabel))); }

        [HarmonyPostfix, HarmonyPatch(typeof(BuddyIconButtonBehavior), nameof(BuddyIconButtonBehavior.DidPressIcon))]
        private static void IconPicked() { Tell.Now("Icon set to " + Icons.Name(ChatValues.GetPlayerBuddyIcon()) + "."); }

        // Filling in your profile: three questions answered with 1, 2, 3.
        [HarmonyPostfix, HarmonyPatch(typeof(BuddyInfoManagerBehavior), nameof(BuddyInfoManagerBehavior.ShowNextProfileButtons))]
        private static void ProfileQuestion(BuddyInfoManagerBehavior __instance)
        {
            int state = Traverse.Create(__instance).Field("infoEditState").GetValue<int>();   // already moved on
            TextLinkButton[] labels = state == 1 ? __instance.profile1Labels : state == 2 ? __instance.profile2Labels : state == 3 ? __instance.profile3Labels : null;
            string info = Ui.Text(__instance.infoLabel);
            if (labels == null)
            {
                Tell.Queue("Profile done. " + Tell.Stop(info));
                return;
            }
            var sb = new StringBuilder();
            foreach (TextLinkButton l in labels)
            {
                string t = Ui.Text(l.GetComponent<TMP_Text>());
                if (t.Length > 0) sb.Append(Tell.Stop(t) + " ");
            }
            string intro = state == 1 ? "Fill in your profile, press 1, 2 or 3. " : "";
            Tell.Queue(intro + (state > 1 && info.Length > 0 ? "Profile so far: " + Tell.Stop(info) + " " : "") + "Choices: " + sb.ToString().Trim());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowBuddyList))]
        private static void BuddyList(ApplicationManagerBehavior __instance)
        {
            var sb = new StringBuilder("Buddy list: ");
            foreach (BuddyListRowBehavior r in __instance.buddyListManager.buddyRows)
                if (!r.IsHidden && !string.IsNullOrEmpty(r.Screenname)) sb.Append(r.Screenname + ", " + Ui.Text(r.statusLabel) + ". ");
            Tell.Queue(sb.ToString());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ApplicationManagerBehavior), nameof(ApplicationManagerBehavior.ShowBuddyInfo))]
        private static void Profile(ApplicationManagerBehavior __instance)
        {
            BuddyInfoManagerBehavior b = __instance.buddyInfoManager;
            string status = Ui.Text(b.statusLabel);
            string icon = PicturePatches.ProfileIcon != BuddyIconType.None ? " Icon: " + Icons.Describe(PicturePatches.ProfileIcon) + "." : "";
            Tell.Now("Profile: " + Ui.Text(b.usernameLabel) + (status.Length > 0 ? ", " + status : "") + "." + icon + " " + Ui.Text(b.infoLabel));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MainMenuManagerBehavior), nameof(MainMenuManagerBehavior.LoadGameScene))]
        private static void Connecting(MainMenuManagerBehavior __instance)
        {
            Tell.Now(Tell.Stop(Ui.Text(__instance.headerLabel)));
        }
    }
}
