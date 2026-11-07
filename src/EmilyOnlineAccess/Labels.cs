using System.Collections.Generic;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmilyIsAwayTooAccess
{
    // What each control in Emily Online is called when the navigator lands on it.
    internal static class Labels
    {
        // Picture-only buttons, by GameObject path ending (lower case). Filled in from the
        // test driver's dump of each screen.
        private static readonly KeyValuePair<string, string>[] ByPath =
        {
            new KeyValuePair<string, string>("systemcontainer/logoutbutton", "Sign out"),
            new KeyValuePair<string, string>("systemcontainer/settingsbutton", "Settings"),
            new KeyValuePair<string, string>("profilewidget/infobutton1", "Buddy's profile"),
            new KeyValuePair<string, string>("profilewidget/infobutton2", "Your profile"),
            new KeyValuePair<string, string>("alertpopup/popup/closebutton", "Close"),
            new KeyValuePair<string, string>("profilewidget/buddyicon1", "Buddy's Steam picture, opens their profile"),
            new KeyValuePair<string, string>("profilewidget/buddyicon2", "Your Steam picture, opens your profile"),
            new KeyValuePair<string, string>("formatwidget/formatemojibutton", "Emotes"),
            new KeyValuePair<string, string>("formatwidget/formattextbutton", "Text colour"),
            new KeyValuePair<string, string>("formatwidget/formatbackgroundbutton", "Background colour"),
            new KeyValuePair<string, string>("chatwindow1/warnbutton", "Warn"),
            new KeyValuePair<string, string>("editcontainer/leftbutton", "Previous profiles"),
            new KeyValuePair<string, string>("editcontainer/rightbutton", "More profiles"),
            new KeyValuePair<string, string>("formatbuttons/formattextbutton", "Text colour"),
            new KeyValuePair<string, string>("formatbuttons/formatbackgroundbutton", "Background colour"),
            new KeyValuePair<string, string>("formatbuttons/formatrichtext", "Rich text, lets you use formatting tags"),
            new KeyValuePair<string, string>("inputcontainer/textmeshpro - inputfield", "Your custom profile text"),
            new KeyValuePair<string, string>("chatwindow1/sendbutton", "Send"),
        };

        public static bool Wanted(Component c) { return true; }

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
                    string text = Tell.Plain(Ui.BoxText(c.Target));
                    return name + ", text box, " + (text.Length == 0 ? "blank" : text);
                case Control.Kind.Link:
                    return name + ", link";
                case Control.Kind.ProfileLink:
                    return name + ", web link";
                default:
                    if (!c.Enabled && IsPickedOption((Button)c.Target)) return name + ", button, selected";
                    return name + ", button" + (c.Enabled ? "" : ", unavailable");
            }
        }

        private static bool IsPickedOption(Button b)
        {
            SettingsManagerBehavior s = Object.FindObjectOfType<SettingsManagerBehavior>();
            return s != null && (System.Array.IndexOf(s.assistButtons, b) >= 0 || System.Array.IndexOf(s.windowButtons, b) >= 0);
        }

        public static string Name(Control c)
        {
            Component t = c.Target;
            if (c.Type == Control.Kind.Link) return Ui.Text(t.GetComponent<TMP_Text>());
            if (c.Type == Control.Kind.ProfileLink) return Traverse.Create(t).Field("linkText").GetValue<string>() ?? "link";

            BuddyListRowBehavior row = t.GetComponentInParent<BuddyListRowBehavior>();
            if (row != null && row.button == t)
                return Ui.Text(row.screennameLabel) + ", " + Ui.Text(row.statusLabel)
                       + (row.notifBackground != null && row.notifBackground.color.a > 0.05f ? ", new message" : "");

            ColorSwatchBehavior swatch = t.GetComponentInParent<ColorSwatchBehavior>();
            if (swatch != null) return ColourName(swatch.color);

            string path = Ui.Path(t.transform).ToLowerInvariant();
            foreach (var kv in ByPath) if (path.EndsWith(kv.Key)) return kv.Value;

            string emote = Emotes.ForButton(t);
            if (emote != null) return emote;

            if (c.Type == Control.Kind.TextBox)
            {
                string ph = Ui.BoxPlaceholder(t);
                if (ph.Length > 0) return ph;
                if (t.GetComponentInParent<ChatManagerBehavior>() != null) return "Message";
                string ctx = Context(t.transform);
                return ctx.Length > 0 ? ctx : Ui.FromName(t.name);
            }

            string under = Ui.TextUnder(t.transform).Trim(',', ' ');
            if (under.Length > 0) return under;
            Transform parent = t.transform.parent;
            if (parent != null && parent.GetComponentsInChildren<Selectable>().Length == 1)
            {
                string beside = Ui.TextUnder(parent).Trim(',', ' ');
                if (beside.Length > 0) return beside;
            }
            string named = Ui.FromName(t.name);
            return named.Length > 0 ? named : Ui.FromName(parent != null ? parent.name : "");
        }

        public static string Context(Transform t)
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
            if (own.Length == 0 && t.parent.GetComponentsInChildren<Selectable>().Length == 1) return "";
            return parts[0] == own ? "" : parts[0];
        }

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
