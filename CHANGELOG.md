# Changelog

## 1.0.3 (2026-11-07)

### Changed

- The profile at the start of each chapter goes one step at a time. After "Icon set to", each question (music, movie, saying) is read on its own when it's your turn, with what you picked, and the arrow keys start on its answers. Each answer says its question ("Movie: 2. the departed"). The buddy list is no longer read out at that point.
- The chats now wait until your profile is finished (or until you leave the profile window), so messages don't talk over the questions.
- Emily Online: "is typing" is now read for any buddy, open chat or not, whenever Steam sends it (it no longer depends on the game showing it). In testing, Steam didn't send typing notices between two Emily Online players, so the game can't show them either.

## 1.0.2 (2026-11-07)

### Added

- A setting to stop the mod reading each letter you type (Emily Online's message box, the sign-in and code boxes, and your own answers), for screen readers that already say typed keys. It's the first item in Emily Online's settings.

## 1.0.1 (2026-11-07)

### Changed

- While a reply is being typed, in a text box, or while typing your own answer, the mod's letter keys (c, t, w, l, i, r, m) and the arrow, Enter and Space keys no longer do mod things; they just type. Hold Ctrl to use them then (Ctrl+C, Ctrl+M, Ctrl+Up and so on). Page Up and Page Down still read the chat, and no longer type a letter.

## 1.0 (2026-11-07)

The first release. Emily is Away Too can be played from start to finish with a screen reader: all five chapters with both story paths, the ending, the icon gallery and secret buddies. Emily Online, the messenger that comes with the game, is accessible too.

### Added

- Speech and braille through Prism: NVDA, JAWS and other screen readers, or Windows' own voices.
- The fake installer on first start: every page is read and the desktop background choice works from the keyboard.
- Main menu, chapter pages, the game's settings page and pop-up alerts are read and work from the keyboard (Up, Down, Home, End, Enter, Space; Left and Right for the sound slider).
- Sign-in: the screen name and first name boxes read back each letter; Randomize works from the keyboard.
- Buddy icons: a name and a short picture description for every icon, in the icon picker, on the chat windows and in profiles.
- Picture descriptions for the installer's desktop backgrounds, the chapter page pictures, each chapter's banner ad, and Emily's Chapter 4 art project.
- Profile: the three profile questions are read with their choices and your profile so far.
- Chat: every message in both windows is read as it arrives, along with sign-ons, away messages, "is typing...", file transfers and captions like "emily will remember that". Page Up, Page Down, Ctrl+Home and Ctrl+End read back through the chat you're in.
- Two windows: Tab switches between Emily's and Evelyn's chats, and also opens the waiting chat when none is open yet or when you're in a secret buddy's chat. Switching says whose window it is and what's waiting there. w tells you what's going on in each window.
- Replies: the three choices are read and c repeats them. While you type, each word is read, erased text is read as "erased ...", and the finished message is read before you press Enter.
- Reply timers: "timed reply" with the number of seconds, warnings at 10 and 5 seconds, and t for the time left. Timers are twice as long by default and can be made longer or switched off.
- Typed answers (like a nickname): each letter is read and the answer is read when sent.
- Links: "sent a link to ..." and l opens the newest link. File transfers put the focus on Accept.
- Buddy statuses and the buddy list are announced; profiles are read when opened, and i opens your buddy's profile.
- Pop-up questions (like signing out mid-chapter, replaying or quitting) read their message and start on Cancel.
- Colour pickers: each colour is read by name, and picking one closes the picker.
- The ending: the title card and every page are read, including the unlocked icon by name and the thanks buddy list.
- Icon gallery: unlocked icons with descriptions, locked icons with the game's own hints, page names, and the Enter code box with its result.
- New icon unlocks and Steam achievements are announced.
- Export Log says when the chat logs are saved to the desktop.
- Emily Online Access, for the Emily Online messenger that comes with the game: the buddy list of Steam friends with statuses, chats with every message read, the message box with each letter read, the eight emotes by name, warnings, profiles and profile editing, the game's settings, and its own mod settings on Ctrl+M.
- An installer (Loadstone) that also installs BepInEx in the game folder and in the Emily Online folder.
- Mod settings on m: reply timers, timer warnings, reading the other window, reading words, erased text, the finished message, and "is typing". The game pauses while the list is open.
