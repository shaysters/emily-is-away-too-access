using System;
using System.Collections.Generic;
using EmilyIsAwayTooAccess.Speech;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmilyIsAwayTooAccess
{
    // One thing on screen the player can press or change.
    internal class Control
    {
        public enum Kind { Button, Toggle, Slider, TextBox, Link, ProfileLink }

        public Kind Type;
        public Component Target;
        public GameObject Go => Target.gameObject;
        public bool Enabled = true;
        public Vector2 Pos;
        public int Group;       // 0 = the main content, 1 = side lists and system buttons (read last)
    }

    // Keyboard access to the game's mouse-only windows. It finds every visible button,
    // checkbox, slider, text box and text link, puts them in reading order (top to
    // bottom, left to right) and lets the player move with Up/Down, Home/End and press
    // with Enter or Space. Left/Right change sliders. When a pop-up is open, only its
    // controls are in the list.
    internal class Navigator : MonoBehaviour
    {
        public static Navigator Instance { get; private set; }
        private readonly List<Control> _list = new List<Control>();
        private GameObject _focus;
        private int _index = -1;
        private string _boxText;          // text box contents, to echo typing
        private Selectable _box;

        private void Awake() { Instance = this; }

        private void Update()
        {
            try
            {
                if (SettingsMenu.HoldsKeys) return;
                EchoTextBox();
                bool down = Input.GetKeyDown(KeyCode.DownArrow), up = Input.GetKeyDown(KeyCode.UpArrow);
                bool home = Input.GetKeyDown(KeyCode.Home) && !Keys.Ctrl(), end = Input.GetKeyDown(KeyCode.End) && !Keys.Ctrl();
                bool left = Input.GetKeyDown(KeyCode.LeftArrow), right = Input.GetKeyDown(KeyCode.RightArrow);
                bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
                bool space = Input.GetKeyDown(KeyCode.Space);
                if (!(down || up || home || end || left || right || enter || space)) return;
                if (ChatState.TypingReply() && !Keys.Ctrl()) return;   // every key types the reply

                Selectable box = FocusedBox();
                if (box != null && (left || right || space || home || end)) return;   // keys for editing the text
                if (box != null && enter) return;                                     // the game handles Enter in its text boxes

                if (down || up || home || end)
                {
                    Refresh();
                    if (_list.Count == 0) { Tell.Now("Nothing to select here."); Keys.UseKey(); return; }
                    if (down) Step(1);
                    else if (up) Step(-1);
                    else if (home) Go(0);
                    else Go(_list.Count - 1);
                    Keys.UseKey();
                    return;
                }

                Control c = Current();
                if (c == null) return;
                if (enter && ChatState.EnterSendsNow()) return;   // a finished reply: Enter is the game's Send
                if ((left || right) && c.Type == Control.Kind.Slider) { Nudge((Slider)c.Target, right ? 1 : -1); Keys.UseKey(); return; }
                if (enter || space) { Press(c); Keys.UseKey(); }
            }
            catch (Exception e) { Plugin.Log.LogError("Navigator: " + e); }
        }

        // --- The list ---

        public List<Control> Refresh()
        {
            _list.Clear();
            Transform modal = ScreenWatch.ModalRoot();
            foreach (Selectable s in FindObjectsOfType<Selectable>())
            {
                if (s is Scrollbar || !s.IsActive()) continue;
                Control.Kind kind = s is Toggle ? Control.Kind.Toggle : s is Slider ? Control.Kind.Slider
                    : Ui.IsBox(s) ? Control.Kind.TextBox : Control.Kind.Button;
                Add(s, kind, s.IsInteractable(), modal);
            }
            foreach (TextLinkButton l in FindObjectsOfType<TextLinkButton>())
            {
                TMP_Text t = l.GetComponent<TMP_Text>();
                if (t != null && Ui.Text(t).Length > 0) Add(l, Control.Kind.Link, true, modal);
            }
            foreach (MenuBuddyIconBehavior m in FindObjectsOfType<MenuBuddyIconBehavior>())   // icon gallery
                Add(m, Control.Kind.Button, true, modal);
            foreach (InfoLinkButton l in FindObjectsOfType<InfoLinkButton>())
                if (Traverse.Create(l).Field("isActive").GetValue<bool>()) Add(l, Control.Kind.ProfileLink, true, modal);

            // Reading order: main content before side panels, then rows from the top,
            // then left to right.
            _list.Sort((a, b) =>
            {
                if (a.Group != b.Group) return a.Group.CompareTo(b.Group);
                if (Mathf.Abs(a.Pos.y - b.Pos.y) > 8f) return b.Pos.y.CompareTo(a.Pos.y);
                return a.Pos.x.CompareTo(b.Pos.x);
            });
            _index = _focus == null ? -1 : _list.FindIndex(c => c.Go == _focus);
            return _list;
        }

        private void Add(Component c, Control.Kind kind, bool enabled, Transform modal)
        {
            Transform t = c.transform;
            if (!t.gameObject.activeInHierarchy || !Ui.Shown(t) || !Ui.Clickable(t)) return;
            if (modal != null && !t.IsChildOf(modal)) return;
            if (!Labels.Wanted(c)) return;
            int group = Ui.Path(t).Contains("/SystemContainer/") ? 2 : SideGroup(t) ? 1 : 0;   // sign out and settings last
            _list.Add(new Control { Type = kind, Target = c, Enabled = enabled, Pos = Ui.ScreenPos(t), Group = group });
        }

        // The menu's chapter list and the buddy list on the left: read after the page itself.
        private static readonly string[] SideParts = { "/MenuList/", "/BuddyList/" };
        private static bool SideGroup(Transform t)
        {
            string p = Ui.Path(t);
            foreach (string s in SideParts) if (p.Contains(s)) return true;
            return false;
        }

        private Control Current()
        {
            if (_focus == null || !_focus.activeInHierarchy) return null;
            Refresh();
            return _index >= 0 ? _list[_index] : null;
        }

        private void Step(int by)
        {
            int i = _index < 0 ? (by > 0 ? 0 : _list.Count - 1) : _index + by;
            if (i < 0) i = _list.Count - 1;          // wrap around
            if (i >= _list.Count) i = 0;
            Go(i);
        }

        private void Go(int i)
        {
            _index = i;
            Control c = _list[i];
            _focus = c.Go;
            FocusBox(c);
            Tell.Now(WithContext(c));
        }

        private string _context = "";

        // Says the group caption ("Type Assist") when moving into a new group.
        private string WithContext(Control c)
        {
            string ctx = c.Type == Control.Kind.TextBox ? "" : Labels.Context(c.Target.transform);
            string said = ctx.Length > 0 && ctx != _context ? ctx + ": " : "";
            _context = ctx;
            return said + Labels.Describe(c);
        }

        // Puts the focus on a control without moving the player's place if there isn't one.
        public void FocusFirst(bool speak)
        {
            Refresh();
            if (_list.Count == 0) { _focus = null; return; }
            _index = 0;
            _focus = _list[0].Go;
            FocusBox(_list[0]);
            _context = "";
            if (speak) Tell.Queue(WithContext(_list[0]));
        }

        public void FocusOn(GameObject go, bool speak)
        {
            Refresh();
            int i = _list.FindIndex(c => c.Go == go);
            if (i < 0) return;
            _index = i; _focus = go;
            FocusBox(_list[i]);
            _context = "";
            if (speak) Tell.Queue(WithContext(_list[i]));
        }

        public void Forget() { _focus = null; _index = -1; }

        // --- Pressing ---

        public void Press(Control c)
        {
            if (!c.Enabled) { Tell.Now("Unavailable."); return; }
            switch (c.Type)
            {
                case Control.Kind.Link:
                    ((TextLinkButton)c.Target).onClickEvent?.Invoke();
                    break;
                case Control.Kind.ProfileLink:
                    string url = Traverse.Create(c.Target).Field("urlLink").GetValue<string>();
                    SoundManagerBehavior.Instance?.PlayMouseClick();
#if !EMILY_ONLINE
                    ApplicationManagerBehavior.Instance?.PlayerClickedInfoLink(url);
#endif
                    Tell.Now("Opening the link in your web browser.");
                    Application.OpenURL(url);
                    break;
                case Control.Kind.TextBox:
                    FocusBox(c);
                    Tell.Now("Editing. " + Labels.Describe(c));
                    break;
                case Control.Kind.Slider:
                    Tell.Now(Labels.Describe(c) + ". Use Left and Right to change it.");
                    break;
                case Control.Kind.Toggle:
                    Click(c.Go);
                    Tell.Queue(((Toggle)c.Target).isOn ? "checked" : "not checked");
                    break;
                default:
                    Click(c.Go);
                    break;
            }
        }

        public static void Click(GameObject go)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(go, data, ExecuteEvents.pointerClickHandler);
        }

        private static void Nudge(Slider s, int dir)
        {
            float step = s.wholeNumbers ? 1f : (s.maxValue - s.minValue) / 10f;
            s.value = Mathf.Clamp(s.value + dir * step, s.minValue, s.maxValue);
            Tell.Now(Labels.SliderValue(s));
        }

        // --- Text boxes ---

        private void FocusBox(Control c)
        {
            Selectable box = c.Type == Control.Kind.TextBox ? (Selectable)c.Target : null;
            if (box != null) Ui.ActivateBox(box);
            else if (FocusedBox() != null)
            {
                Ui.DeactivateBox(FocusedBox());
                EventSystem.current?.SetSelectedGameObject(null);
            }
            _box = box;
            _boxText = box == null ? null : Ui.BoxText(box);
        }

        // The text box (Unity or TextMesh Pro) that has the keyboard, if any.
        public static Selectable FocusedBox()
        {
            GameObject sel = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (sel == null) return null;
            foreach (Selectable s in sel.GetComponents<Selectable>())
                if (Ui.IsBox(s) && Ui.BoxFocused(s)) return s;
            return null;
        }

        // Screen readers can't see Unity's text boxes, so say each typed or deleted letter.
        private void EchoTextBox()
        {
            Selectable box = FocusedBox();
            if (box != _box) { _box = box; _boxText = box == null ? null : Ui.BoxText(box); return; }
            if (box == null || Ui.BoxText(box) == _boxText) return;
            string old = _boxText ?? "";
            string now = Ui.BoxText(box) ?? "";
            _boxText = now;
            if (now.Length == old.Length + 1 && now.StartsWith(old)) Tell.Now(Keys.SayChar(now[now.Length - 1]));
            else if (old.Length == now.Length + 1 && old.StartsWith(now)) Tell.Now(Keys.SayChar(old[old.Length - 1]) + " deleted");
            else if (now.Length > old.Length && now.StartsWith(old)) Tell.Now(now.Substring(old.Length));   // an emote or pasted text
            else Tell.Now(now.Length == 0 ? "blank" : now);
        }
    }
}
