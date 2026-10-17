# osu-gamma

A small neon gamma booster for Windows, made for brightening osu!.

<img src="icon.png" width="96">

- Drag the slider (0,50 to 3,00) to change screen gamma live, on all monitors
- Presets: 1,00 / 1,15 / 1,30 / 1,50 / 1,80
- RESET or closing the app puts gamma back to 1,00

## Download

Grab `GammaTool.exe` from [Releases](../../releases). No install needed; runs on Windows 10/11.

## Notes

- Play osu! in **borderless** or windowed mode. Exclusive fullscreen can override the gamma.
- If the app is force-closed, reopen it and press RESET (or sign out and back in).
- Some PCs limit how far gamma can go. If a value is blocked, an **UNLOCK** button appears. It sets
  `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ICM\GdiIcmGammaRange = 256` (asks for admin);
  sign out and back in afterwards.

## Build

```
dotnet build -c Release -o dist
```

The icon is generated with `powershell -File make-icon.ps1 -OutIco icon.ico -OutPreview icon.png`.
