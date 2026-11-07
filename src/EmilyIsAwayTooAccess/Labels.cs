using System;
using System.Collections.Generic;
using System.Linq;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmilyIsAwayTooAccess
{
    // What each control is called when the navigator lands on it.
    internal static class Labels
    {
        // Picture-only buttons, by GameObject path ending (lower case).
        private static readonly KeyValuePair<string, string>[] ByPath =
        {
            new KeyValuePair<string, string>("menulist/systemcontainer/logoutbutton", "Shut down"),
            new KeyValuePair<string, string>("menulist/systemcontainer/settingsbutton", "Settings"),
            new KeyValuePair<string, string>("signincontainer/sendbutton", "Sign in"),
            new KeyValuePair<string, string>("buddylist/systemcontainer/logoutbutton", "Sign out"),
            new KeyValuePair<string, string>("buddylist/systemcontainer/settingsbutton", "Settings"),
            new KeyValuePair<string, string>("alertpopup/popup/closebutton", "Close"),
            new KeyValuePair<string, string>("codepopup/popup/closebutton", "Close"),
            new KeyValuePair<string, string>("exportpopup/popup/closebutton", "Close window"),
            new KeyValuePair<string, string>("entrycontainer/inputfield", "Code"),
            new KeyValuePair<string, string>("iconcontainer/leftbutton", "Previous page"),
            new KeyValuePair<string, string>("iconcontainer/rightbutton", "Next page"),
            new KeyValuePair<string, string>("profilewidget/infobutton1", "Buddy's profile"),
            new KeyValuePair<string, string>("profilewidget/infobutton2", "Your profile"),
            new KeyValuePair<string, string>("formatwidget/formattextbutton", "Text colour"),
            new KeyValuePair<string, string>("formatwidget/formatbackgroundbutton", "Background colour"),
            new KeyValuePair<string, string>("formatbuttons/formattextbutton", "Text colour"),
            new KeyValuePair<string, string>("formatbuttons/formatbackgroundbutton", "Background colour"),
        };

        public static bool Wanted(Component c)
        {
            return true;
        }

        public static string Describe(Control c)
        {
            string name = Name(c);
            switch (c.Type)
            {
                case Control.Kind.Toggle:
                    return name + ", checkbox, " + (((Toggle)c.Target).isOn ? "checked" : "not checked");
                case Control.Kind.Slider:
                    return name + ", slider, " + SliderValue((Slider)c.Target);
                case Control.Kind.TextBox:
                    string text = Ui.BoxText(c.Target);
                    return name + ", text box, " + (string.IsNullOrEmpty(text) ? "blank" : text);
                case Control.Kind.Link:
                    return name + ", link";
                case Control.Kind.ProfileLink:
                    return name + ", web link";
                default:
                    if (!c.Enabled && IsPickedOption((Button)c.Target)) return name + ", button, selected";
                    if (!c.Enabled && c.Target.GetComponentInParent<MenuListRowBehavior>() != null) return name + ", locked";
                    return name + ", button" + (c.Enabled ? "" : ", unavailable");
            }
        }

        // Buttons the game greys out to show which one is picked (like radio buttons).
        private static bool IsPickedOption(Button b)
        {
            SettingsManagerBehavior s = b.GetComponentInParent<SettingsManagerBehavior>() ?? UnityEngine.Object.FindObjectOfType<SettingsManagerBehavior>();
            if (s != null && (s.assistButtons.Contains(b) || s.windowButtons.Contains(b))) return true;
            InstallerDesktopBehavior d = UnityEngine.Object.FindObjectOfType<InstallerDesktopBehavior>();
            return d != null && (b == d.button1 || b == d.button2 || b == d.button3);
        }

        public static string Name(Control c)
        {
            Component t = c.Target;
            if (c.Type == Control.Kind.Link) return Ui.Text(t.GetComponent<TMP_Text>());
            if (c.Type == Control.Kind.ProfileLink) return Traverse.Create(t).Field("linkText").GetValue<string>() ?? "link";

            BuddyListRowBehavior row = t.GetComponentInParent<BuddyListRowBehavior>();
            if (row != null && row.button == t && !string.IsNullOrEmpty(row.Screenname)) return row.Screenname + ", " + Ui.Text(row.statusLabel);

            ColorSwatchBehavior swatch = t.GetComponentInParent<ColorSwatchBehavior>();
            if (swatch != null) return ColourName(swatch.color);

            MenuBuddyIconBehavior gallery = t as MenuBuddyIconBehavior;
            if (gallery != null)
            {
                var tg = Traverse.Create(gallery);
                BuddyIconType gt = tg.Field("iconType").GetValue<BuddyIconType>();
                if (!tg.Field("isUnlocked").GetValue<bool>())
                {
                    // The game shows a hint for some locked icons when the mouse is over them.
                    string hint = Tell.Plain(ChatIcons.GetHintForBuddyIcon(gt));
                    return "locked icon" + (hint.Length > 0 ? ", hint: " + hint : "");
                }
                return Icons.Describe(gt) + (gt == ChatValues.GetPlayerBuddyIcon() ? ", your icon" : "");
            }

            BuddyIconButtonBehavior icon = t.GetComponentInParent<BuddyIconButtonBehavior>();
            if (icon != null && icon.buddyButton == t)
            {
                var tr = Traverse.Create(icon);
                if (!tr.Field("isUnlocked").GetValue<bool>()) return "locked icon";
                BuddyIconType type = tr.Field("iconType").GetValue<BuddyIconType>();
                return Icons.Describe(type) + (type == ChatValues.GetPlayerBuddyIcon() ? ", your icon" : "");
            }

            // The two pictures at the side of a chat window.
            ChatManagerBehavior cm = t.GetComponentInParent<ChatManagerBehavior>();
            if (cm != null)
            {
                Image img = t.GetComponent<Image>();
                if (img != null && img == cm.otherIcon)
                {
                    BaseScriptBehavior sc = Traverse.Create(cm).Property("Script").GetValue<BaseScriptBehavior>();
                    if (sc != null) return "Buddy's icon, " + Icons.Describe(sc.OtherBuddyIcon);
                }
                if (img != null && img == cm.playerIcon) return "Your icon, " + Icons.Describe(ChatValues.GetPlayerBuddyIcon());
            }

            // The installer's desktop background choices: the caption plus the picture.
            InstallerDesktopBehavior desk = UnityEngine.Object.FindObjectOfType<InstallerDesktopBehavior>();
            if (desk != null && (t == desk.button1 || t == desk.button2 || t == desk.button3))
            {
                int n = t == desk.button1 ? 1 : t == desk.button2 ? 2 : 3;
                return Ui.TextUnder(t.transform.parent).Trim(',', ' ') + ", " + Pictures.Desktop(n);
            }

            string path = Ui.Path(t.transform).ToLowerInvariant();
            foreach (var kv in ByPath) if (path.EndsWith(kv.Key)) return kv.Value;

            if (c.Type == Control.Kind.TextBox)
            {
                string ph = Ui.BoxPlaceholder(t);
                if (ph.Length > 0) return ph;
                string ctx = Context(t.transform, true);
                return ctx.Length > 0 ? ctx : Ui.FromName(t.name);
            }

            string under = Ui.TextUnder(t.transform).Trim(',', ' ');
            if (under.Length > 0) return under;
            // A picture button whose caption sits next to it (Choice1/Button + Choice1/Label).
            Transform parent = t.transform.parent;
            if (parent != null && parent.GetComponentsInChildren<Selectable>().Length == 1)
            {
                string beside = Ui.TextUnder(parent).Trim(',', ' ');
                if (beside.Length > 0) return beside;
            }
            string named = Ui.FromName(t.name);
            return named.Length > 0 ? named : Ui.FromName(parent != null ? parent.name : "");
        }

        // The caption of the group a control is in ("Type Assist" for its On and Off
        // buttons): short texts beside it in its parent that aren't part of any control.
        public static string Context(Transform t, bool forTextBox = false)
        {
            Transform parent = t.parent;
            if (parent == null) return "";
            var parts = new List<string>();
            foreach (TMP_Text x in parent.GetComponentsInChildren<TMP_Text>())
            {
                if (x.transform.IsChildOf(t) || x.GetComponentInParent<Selectable>() != null) continue;
                if (x.GetComponent<TextLinkButton>() != null || x.GetComponent<InfoLinkButton>() != null) continue;
                if (x.transform.parent != parent || !Ui.Shown(x.transform)) continue;
                string s = Ui.Text(x);
                if (s.Length == 0 || s.Length > 40) continue;
                parts.Add(s);
            }
            if (parts.Count != 1) return "";
            string own = Ui.TextUnder(t);
            if (!forTextBox && own.Length == 0 && t.parent.GetComponentsInChildren<Selectable>().Length == 1) return "";   // it's the button's own caption
            return parts[0] == own ? "" : parts[0];
        }

        // A plain colour name for a swatch ("dark blue", "light pink", "grey").
        public static string ColourName(Color c)
        {
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            if (v < 0.12f) return "black";
            if (s < 0.12f) return v > 0.92f ? "white" : v > 0.7f ? "light grey" : v > 0.35f ? "grey" : "dark grey";
            float deg = h * 360f;
            string name;
            if (deg < 15 || deg >= 340) name = "red";
            else if (deg < 40) name = v < 0.6f ? "brown" : "orange";
            else if (deg < 65) name = "yellow";
            else if (deg < 90) name = "lime";
            else if (deg < 160) name = "green";
            else if (deg < 185) name = "teal";
            else if (deg < 200) name = "cyan";
            else if (deg < 250) name = "blue";
            else if (deg < 285) name = "purple";
            else if (deg < 320) name = "magenta";
            else name = "pink";
            if (v < 0.45f) return "dark " + name;
            if (s < 0.45f && v > 0.8f) return "light " + name;
            return name;
        }

        public static string SliderValue(Slider s)
        {
            float range = s.maxValue - s.minValue;
            int pct = range <= 0 ? 0 : Mathf.RoundToInt((s.value - s.minValue) / range * 100f);
            return pct + " percent";
        }
    }
}
