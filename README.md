# Emily is Away Too Access

Emily is Away Too Access is a mod that makes Emily is Away Too, the instant-messenger story game by Kyle Seeley, playable with a screen reader. It reads every chat message in both chat windows, the reply choices, what your character types (and erases), reply timers, profiles, away messages and the little captions like "emily will remember that". It makes the installer, menus, sign-in window, buddy icon settings, buddy list, profiles and pop-ups work from the keyboard, and it describes every buddy icon. If your screen reader has a braille display connected, everything the mod reads goes to braille too.

It works with NVDA, JAWS and other Windows screen readers, and falls back to Windows' built-in voices if no screen reader is running.

It also makes Emily Online accessible: the real instant messenger that comes with the game, for chatting with your Steam friends who own it. You hear your buddy list of Steam friends with their statuses, every message as it arrives, each letter as you type in the message box, the emotes by name, warnings and profiles.

- Join the Discord to chat, get help or report problems: <https://discord.gg/KNDXhztGGK>
- Support Shaysters on Ko-fi: <https://ko-fi.com/shaysters>

## Requirements

- Emily is Away Too for Windows, from Steam: <https://store.steampowered.com/app/523780/Emily_is_Away_Too/>
- A screen reader such as NVDA or JAWS (optional; without one, the mod uses Windows' built-in speech).
- For Emily Online: Steam running, Steam chat allowed on your account, and friends who own the game to chat with.

## Installing

1. Close Emily is Away Too if it's running.
2. Download **EmilyIsAwayTooAccessInstaller.exe** from the [latest release](https://github.com/shaysters/emily-is-away-too-access/releases/latest).
3. Run it. It finds your Steam copy of Emily is Away Too by itself. If it can't, use the Browse button next to the game folder and choose the folder that contains `EmilyIsAwayToo.exe`.
4. Choose **Install**. The installer also installs BepInEx, the mod loader the mod needs, both in the game folder and in its `EmilyOnline` folder.
5. Start Emily is Away Too from Steam as usual. Steam asks whether to play Emily is Away Too or Emily Online. Either way, the mod says it's loaded: "Emily is Away Too Access 1.0 loaded" or "Emily Online Access 1.0 loaded".

If the installer won't start and mentions `VCRUNTIME140.dll`, install the [Microsoft Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe) and try again.

### Updating and uninstalling

Keep the installer. Run it again to update to the newest version, or to uninstall. Close the game first.

### Installing by hand

If you'd rather not use the installer:

1. Install BepInEx 5.4.23.5 for **32-bit** Windows: download `BepInEx_win_x86_5.4.23.5.zip` from the [BepInEx releases](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5) and extract it into the game folder. For Emily Online, extract the same zip a second time into the `EmilyOnline` folder inside the game folder.
2. Download **EmilyIsAwayTooAccess.zip** from the same release as the installer and extract it into the game folder. It puts Emily is Away Too Access in `BepInEx\plugins` and Emily Online Access in `EmilyOnline\BepInEx\plugins`.

The game folder is the one that contains `EmilyIsAwayToo.exe`. On a default Steam install that's:

```
C:\Program Files (x86)\Steam\steamapps\common\Emily is Away Too
```

To uninstall a manual install, delete the `BepInEx\plugins\EmilyIsAwayTooAccess` and `EmilyOnline\BepInEx\plugins\EmilyOnlineAccess` folders and the files `EmilyIsAwayTooAccess-README.md` and `EmilyIsAwayTooAccess-CHANGELOG.md` from the game folder. To remove BepInEx as well, also delete the `BepInEx` folder, `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and `changelog.txt`, in both the game folder and the `EmilyOnline` folder. The mod doesn't change any of the game's own files.

## Playing

The game is two chats at once, with Emily and with Evelyn, over the school year from 2006 to 2007.

- **First start:** the game opens with a fake installer. Each page is read, and you pick a desktop background; each background's picture is described. The game really does save that picture to your Windows desktop.
- **Pictures:** besides the buddy icons, the mod describes the picture at the top of each chapter's page, the banner ad that plays beside the sign-in window and icon picker, and the art project Emily sends in Chapter 4. Profiles say the buddy's icon.
- **Chat:** every message is read as it arrives ("punk4eva: yoooo sam!"), along with sign-ons, away messages, "is typing..." and the captions under the chat. Messages in the window you're not looking at are read too (you can turn that off).
- **Two windows:** press **Tab** to switch between Emily's and Evelyn's windows. When the chats start, Tab also opens the window that's waiting for you.
- **Secret buddies:** some secret icons add a secret buddy to your buddy list. Open their chat from the buddy list (Up and Down, then Enter). Tab takes you back to Emily's or Evelyn's window. Switching says whose window you're in and reads the waiting choices or the newest message.
- **Replying:** you hear the three choices, for example "Reply with 1. hey. 2. hello. 3. yoooo." Press 1, 2 or 3. Then press any keys to type the reply, one letter per key press, just like the game. Each word is read as it's typed. When your character erases something, you hear "erased" and what was erased. When the message is finished you hear "Ready to send" and the whole message; press **Enter**.
- **Reply timers:** some replies have a time limit. You hear "timed reply" and how many seconds you have, and warnings at 10 and 5 seconds. By default the mod makes timers twice as long; you can change that or switch timers off in the settings.
- **Typing an answer:** a few times the game lets you type your own short answer, like a nickname. Type it with the letter keys (each letter is read), Backspace erases, Enter sends.
- **Links:** when someone sends a link, you hear "sent a link to ..." Press **L** to open the newest link in your web browser. The story notices whether you opened it.
- **Files:** when someone sends a file, Accept is selected; press Enter to accept, or Down to Decline. Accepted files really are saved to your Windows desktop by the game.
- **Sign-in (Chapter 1):** type a screen name and your first name, or use the Randomize links. Each letter is read back as you type.
- **Buddy icons:** at the start of each chapter you pick a buddy icon. Each icon is read with its name and a short description of the picture.
- **Profile:** after the icon, you fill in your profile by answering three questions with 1, 2 or 3. Each step reads the choices and your profile so far.
- **Buddy list and profiles:** the buddy list is read when it appears, and statuses are announced as they change. Press Enter on a buddy to hear their profile.
- **Colours:** the Text colour and Background colour buttons on your profile and chat windows open a colour picker. Each colour is read by name; pressing one sets it and closes the picker.
- **End of a chapter:** when the chats are over, you hear that you can sign out. Find **Sign out** in the controls (End takes you near it) and press Enter. Signing out in the middle of a chapter asks first; the question starts on Cancel.
- **The ending:** after Chapter 5 you hear the title card and each page: your thank-you, any icon you unlocked (by name), and the buddy list of people the developer thanks, read name by name. The game opens a song in your web browser at the start of the ending.
- **Icon gallery:** on the main menu, the Gallery lists every icon. Unlocked icons are read with their description; locked ones say "locked icon" and the game's hint for it, where it has one. Pressing an unlocked icon makes it yours. Enter code opens a box to type a secret code; you hear whether it worked and what it unlocked.
- **Export Log:** on a finished chapter's page, Export Log saves both chats of that chapter as web pages on your Windows desktop. You hear when it's done.
- **Unlocks and achievements:** new buddy icons are announced by name, and when the game is started from Steam, new achievements are announced too.

## Emily Online

Emily is Away Too comes with Emily Online, a real instant messenger for chatting with your Steam friends who also own the game. When you press Play in Steam, Steam asks which one to start. Emily Online has its own part of the mod, Emily Online Access, which the installer sets up too.

- **Starting:** you hear "Emily Online", the welcome text, and how many of your Steam friends are in your buddy list and online.
- **Buddy list:** Up and Down move through your friends; each is read with their status (online, away, in-game, snooze, offline) and "new message" if they've written to you. Enter opens a chat.
- **Chatting:** opening a chat says who it's with, their status, and the last line. The message box has the keyboard: just type. Each letter is read as you type, Backspace reads what it erased, and Enter sends. Messages from your buddy are read as they arrive, with their name; "Sent." confirms yours. Messages from other buddies are read too (you can turn that off).
- **Emotes:** the Emotes button opens the game's eight smileys (laughing, yelling, winking, tongue out, cool with sunglasses, smiling, crying, kiss). Pick one and it goes into your message. Emotes in messages are read by name.
- **Warn:** the Warn button warns your buddy, the way old messengers did. If buddies warn you too much you can't send for a minute; your warning level is read when it changes.
- **Profiles:** Buddy's profile and Your profile read the profile. On yours, "edit profile" lets you pick a ready-made profile (each one is read when you pick it) or write your own, then "save profile".
- **Keys and settings:** see the Emily Online parts of Keys and Settings below.

## Keys

| Key | Where | What it does |
|---|---|---|
| Up and Down | Windows and menus | Move between the controls on screen. |
| Home and End | Windows and menus | Jump to the first or last control. |
| Enter or Space | Windows and menus | Press the focused control. |
| Left and Right | Sliders | Change the value. |
| 1, 2, 3 | Chat and profile | Pick a reply or a profile line (the game's own keys). |
| Any key | Chat | Type your reply, one letter per key press (the game's own keys). |
| Enter | Chat | Send your reply when it's ready (the game's own key). |
| Tab | Chat | Switch between the two chat windows. |
| Page Up and Page Down | Chat | Read the previous or next line of the chat you're in. |
| Ctrl+Home and Ctrl+End | Chat | Read the first or latest line. |
| c | Chat | Repeat the reply choices. |
| t | Chat | Time left on a reply timer. |
| w | Chat | Which window you're in, and what's waiting in each one. |
| l | Chat | Open the newest link in this chat, or the one picked with Shift+l. |
| Shift+l | Chat | Step back through older links in this chat. |
| i | Chat | Open your buddy's profile. |
| r | Everywhere | Repeat the last thing the mod said. |
| m | Everywhere | Open or close the mod's settings. |

In Emily Online the message box has the keyboard, so the mod's letter keys go with Ctrl there:

| Key | Where | What it does |
|---|---|---|
| Up and Down, Home and End | Emily Online | Move through the buddy list and the window's controls. |
| Enter | Buddy list | Open a chat with the buddy. |
| Any key | Message box | Type your message; each letter is read. |
| Enter | Message box | Send the message (the game's own key). |
| Page Up and Page Down | Chat | Read the previous or next line of the chat. |
| Ctrl+Home and Ctrl+End | Chat | Read the first or latest line. |
| Ctrl+R | Emily Online | Repeat the last thing the mod said. |
| Ctrl+W | Chat | Who you're chatting with, their status, and what you've typed so far. |
| Ctrl+I | Chat | Open your buddy's profile. |
| Ctrl+M | Emily Online | Open or close the mod's settings. |

Notes:

- While a reply is being typed (from picking it until it's sent), every key types it, and the mod's keys stay quiet. To use a mod key then, hold Ctrl: Ctrl+C, Ctrl+T, Ctrl+W, Ctrl+L, Ctrl+I, Ctrl+R and Ctrl+M work, and so do Ctrl with Up, Down, Home and End to move around the controls. The same goes while you're typing in a text box or typing your own answer. Page Up and Page Down always read back through the chat.
- When a reply is ready to send, Enter sends it.
- Keys that belong to your screen reader or Windows never type your reply: the NVDA key (Insert or Caps Lock), Shift, Ctrl, Alt, the Windows key, Escape, Tab and the F keys. So NVDA commands work while you type, including in NVDA's sleep mode.

## Settings

Press **m** anywhere in the game, or **Ctrl+M** while you're typing (a reply, a text box or your own answer). Use Up and Down to move. Enter, Space, Left or Right changes a setting. Escape or **m** closes the list. The game is paused while the list is open: reply timers stop, and your keys don't type into your reply. Changes are saved immediately.

- **Reply timers** (twice as long by default): off (no time limit), normal length, twice as long, or three times as long. Letting a timer run out is part of the story in places, so with timers off you can never miss a reply.
- **Warn when a reply timer is running out** (on): say "10 seconds left" and "5 seconds left".
- **Read messages in the window you're not looking at** (on): with this off you only hear "New message from ..." once, until you switch to that window.
- **Read each word while typing** (on).
- **Read erased text** (on).
- **Read the finished message before sending** (on): say "Ready to send" and the whole message. With this off, your message is read when it's sent instead.
- **Read when a buddy is typing** (on): read "is typing..." and "is deleting...".
- **Read each letter you type in text boxes and your own answers** (on): in the sign-in boxes, the code box and answers like the nickname. If your screen reader already says the keys you type, turn this off to stop hearing each letter twice.

The game's own settings (window size, type assist, sounds, reinstall) are on the main menu's Settings button. Type assist makes your character type replies by themself.

### Emily Online settings

Press **Ctrl+M** in Emily Online (or **m** when you're not in the message box). The keys work the same way, and the message box gets the keyboard back when you close the list.

- **Read each letter as you type** (on): each letter in the message box. If your screen reader already says the keys you type, turn this off to stop hearing each letter twice.
- **Read messages from buddies whose chat isn't open** (on): read them with the buddy's name as they arrive.
- **Read status changes** (only the buddy you're chatting with): when buddies sign in, sign out or go away. You can choose every buddy, or off; with many Steam friends, every buddy can get busy.
- **Read when a buddy is typing** (on).
- **Read your own messages when sent** (off): with this off you hear "Sent." instead.

Emily Online's own Settings button (sounds and window size) is at the bottom of the buddy list.

## Troubleshooting

The mod's settings are in `BepInEx\config\EmilyIsAwayTooAccess-settings.txt` in the game folder, and its log is `BepInEx\LogOutput.log`. Everything the mod says is in the log on lines starting with `SAY`. If something goes wrong, include the log when you report it.

Emily Online keeps its own: `EmilyOnline\BepInEx\config\EmilyOnlineAccess-settings.txt` and `EmilyOnline\BepInEx\LogOutput.log`.

- **No speech at all:** if `BepInEx\LogOutput.log` doesn't exist, BepInEx didn't start: check that `winhttp.dll` is in the game folder (or in `EmilyOnline` for Emily Online) and that you installed the 32-bit BepInEx. Running the installer again fixes this.
- **Emily Online says it can't start:** it needs Steam running, and Steam chat allowed on your account.
- **Achievements:** new Steam achievements are announced, since Steam only shows them as a picture.

To report a problem or suggest something, open an issue on the [GitHub page](https://github.com/shaysters/emily-is-away-too-access/issues).

## Credits

This project was developed by [Shaysters](https://github.com/shaysters). A lot of the code, and the descriptions of the buddy icons, were written with Claude Code, Anthropic's AI coding assistant.

- Speech and braille through [Prism](https://github.com/ethindp/prism) by Ethin Probst, MPL-2.0.
- The installer is built with [Loadstone](https://github.com/bradjrenshaw/loadstone) by [Bradley Renshaw](https://github.com/bradjrenshaw).
- Mods are loaded by [BepInEx](https://github.com/BepInEx/BepInEx).

Emily is Away Too is made by Kyle Seeley. This mod isn't affiliated with or endorsed by Kyle Seeley.

## For developers

Build instructions and how the mod works are in [DEVELOPING.md](https://github.com/shaysters/emily-is-away-too-access/blob/main/DEVELOPING.md).
