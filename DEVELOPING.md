# Developing Emily is Away Too Access

## Building

```bash
dotnet build src/EmilyIsAwayTooAccess -c Release
```

This builds against the game's own DLLs (set `-p:GameDir=...` if the game is elsewhere) and
copies the result into `BepInEx\plugins\EmilyIsAwayTooAccess` in the game folder. Skip the copy
with `-p:SkipDeploy=true`. The game and BepInEx 5 x86 must be installed for the references.
Close the game before building, or the copy can't replace the running DLL.

Release builds for the public repo pass `-p:DevTools=false`, which leaves out `TestDriver.cs`.

## Releasing

1. Set the version in both `Plugin.cs` files and both `.csproj` files (three parts, e.g. `1.0.0`;
   the mod says it without a trailing `.0`, "1.0"). Update `CHANGELOG.md` and write the release
   notes in `changes-latest.md`.
2. `powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1` builds
   `dist\EmilyIsAwayTooAccess.zip` with both mods.
3. `gh release create v1.0.0 --draft --title "Emily is Away Too Access 1.0" --notes-file changes-latest.md dist/EmilyIsAwayTooAccess.zip`
4. `gh workflow run release.yml -f version=v1.0.0`. Loadstone's workflow builds
   `EmilyIsAwayTooAccessInstaller.exe`, writes `loadstone-release.json` and publishes the release.

The installer is [Loadstone](https://github.com/bradjrenshaw/loadstone). Its package files are in
`loadstone-packages/`: the game, BepInEx 5 (32-bit) for the game folder, BepInEx again for the
`EmilyOnline` folder, and the mod, which installs both plugin folders.

The private dev repo has `tools/sync-public.ps1`, which copies everything except the test drivers
and tools into the public repo.

## Emily Online

`src/EmilyOnlineAccess` is the mod for Emily Online, the Unity 5.6 messenger in the `EmilyOnline`
folder. It links the shared files (`Speech/`, `Ui.cs`, `Navigator.cs`, `Icons.cs`) from the main
project and adds its own `Plugin`, `Settings`, `Keys`, `Labels`, `ScreenWatch`, `OnlineChat` and
`Emotes`. Code only for Emily Online (TextMesh Pro text boxes) is behind `#if EMILY_ONLINE`.

```bash
dotnet build src/EmilyOnlineAccess -c Release
```

`tools/run-game.ps1 -Online` runs it with `OnlineTestDriver.cs`. Its sandbox blocks every
message, profile update and warning to real Steam friends (they're only stored locally), and
`fakemsg <text>` pretends the open chat's buddy wrote something. To start `EmilyOnline.exe`
outside Steam, a `steam_appid.txt` containing 523780 is needed next to it.

## Testing

`tools/run-game.ps1` starts the game with the dev build and sends commands to the test driver
(`src/EmilyIsAwayTooAccess/TestDriver.cs`, dev builds only). Everything the mod says is logged
as `SAY:` lines in `BepInEx\LogOutput.log`.

Test runs use a sandbox in `%LOCALAPPDATA%\EmilyIsAwayTooAccessDev\sandbox-<time>`: the game's
save and chat logs, the files the story puts on the desktop and the "seen the installer" flag
all go there, and Steam achievements aren't sent. The player's real save and desktop are never
touched. `-New` starts an empty sandbox; `-From <name>` copies one from
`%LOCALAPPDATA%\EmilyIsAwayTooAccessDev\snapshots\<name>` (for example `after-ch1`).

The game loads its save once into `CustomPrefs.PrefDict`, and its first load happens by a route
the path redirect doesn't catch, so the sandbox reloads that dictionary from the sandbox at
start-up and every frame if it was replaced.

## Engine facts

- Unity 2017.4.40, 32-bit, old Mono (.NET 3.5 profile), Unity UI (uGUI) with TextMesh Pro.
  BepInEx's default entry point works; no special BepInEx.cfg is needed.
- `prism.dll` is the 32-bit build, loaded with `LoadLibraryW` and bound with `GetProcAddress`
  because old Mono can't `[DllImport]` it from the plugin folder.
- Story text is in the `ChapterN...Script` classes (Emily and Evelyn each have their own;
  Chapters 4 and 5 have two versions depending on who you chose). Replies are strings where
  `<` is a backspace and `~` is where Enter sends.
- Three chat windows: `chatManager1` (Emily), `chatManager2` (Evelyn), `chatManager3` (secret
  buddies). Tab only switches 1 and 2.
- Saves are `savefile.txt` and chat logs in `AppData\LocalLow\KyleSeeley\Emily Is Away Too`;
  "seen the installer" is a PlayerPrefs value in `HKCU\Software\KyleSeeley\Emily Is Away Too`.
- The installer saves the chosen background picture to the real desktop, and Chapter 2 saves
  `chatlog.html` there if you accept the file.

## How it works

- `ChatPatches.cs`: Harmony patches for chat lines, reply choices, timers, captions, free
  typing, file offers, window switches and buddy statuses.
- `ChatState.cs`: what the mod knows about each chat window; the review, choices, timer,
  where-am-I and link keys use it.
- `ChatWatch.cs`: polls your reply as it's typed (words, erased text, ready to send), "is
  typing..." and timer warnings.
- `Navigator.cs`, `Labels.cs`, `Ui.cs`: keyboard access to the game's uGUI windows: finds
  visible buttons, text boxes, sliders and text links, orders them (page first, side lists
  last), names them, and presses them.
- `ScreenWatch.cs`: reads pages, pop-ups, profiles, the icon picker and labels that change by
  themselves ("Connecting...").
- `Icons.cs`: names and picture descriptions for every `BuddyIconType`.
- `Pictures.cs`: descriptions of the other pictures (installer backgrounds, chapter headers, banner ads, art project). The test driver's `exportpics <folder>` saves the game's pictures for writing these.
- `Settings.cs`: the settings list on m, saved to `BepInEx/config/EmilyIsAwayTooAccess-settings.txt`;
  pauses the game while open; the Tab key fix.
- `Keys.cs`: review and repeat keys.
- `Speech/`: the Prism wrapper and `Tell`, which logs every line as `SAY:`.
