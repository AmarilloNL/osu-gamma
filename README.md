<div align="center">

<img src="icon.png" width="110">

# 🌈 osu-gamma

**A small neon gamma booster for Windows and Linux, made for brightening osu!.**

[![Latest release](https://img.shields.io/github/v/release/AmarilloNL/osu-gamma?style=for-the-badge&color=ff2fa8)](../../releases/latest)
[![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4?style=for-the-badge&logo=windows11&logoColor=white)](#windows)
[![Linux](https://img.shields.io/badge/Linux-experimental-f59e0b?style=for-the-badge&logo=linux&logoColor=white)](#linux)

[**⬇️ Download**](../../releases/latest) · [🪟 Windows](#windows) · [🐧 Linux](#linux) · [🛠️ Build](#build)

</div>

## ✨ Features

- 🎚️ **Live slider** from 0,50 to 3,00: changes screen gamma instantly, on all monitors
- ⚡ **Presets:** 1,00 · 1,15 · 1,30 · 1,50 · 1,80
- ↩️ **RESET**, or closing the app, puts gamma back to 1,00

<a id="windows"></a>

## 🪟 Windows

Grab **`GammaTool.exe`** from [**Releases**](../../releases/latest). No install needed; runs on Windows 10/11.

> [!TIP]
> Play osu! in **borderless** or windowed mode. Exclusive fullscreen can override the gamma.

- 🔁 If the app is force-closed, reopen it and press **RESET** (or sign out and back in).
- 🔓 Some PCs limit how far gamma can go. If a value is blocked, an **UNLOCK** button appears. It
  sets `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ICM\GdiIcmGammaRange = 256` (asks for
  admin); sign out and back in afterwards.
- 🛡️ Windows SmartScreen may warn because the exe isn't signed: click **More info → Run anyway**.

<a id="linux"></a>

## 🐧 Linux (experimental)

> [!WARNING]
> The Linux version hasn't been tested on a real Linux desktop yet. If something doesn't work,
> please [open an issue](../../issues) with your distro, desktop and the red message the app shows.

**1. Download and run.** Grab **`GammaTool-linux-x64`** from [Releases](../../releases/latest)
(64-bit x86 only). Everything the app needs is bundled; no .NET install required.

```sh
chmod +x GammaTool-linux-x64
./GammaTool-linux-x64
```

**2. Make gamma work on your desktop.** Linux has no single way to change gamma. Check which kind
of session you're in:

```sh
echo $XDG_SESSION_TYPE    # prints x11 or wayland
echo $XDG_CURRENT_DESKTOP # e.g. KDE, GNOME, sway, Hyprland
```

| Your session | Works? | What to do |
|---|---|---|
| **X11** (any desktop) | ✅ Yes | Make sure `xrandr` is installed (see below). |
| **Wayland** on Sway, Hyprland, river or another wlroots compositor | ✅ Yes | Install and run `wl-gammarelay-rs` (see below). |
| **Wayland** on KDE Plasma or GNOME | ❌ No | These don't let apps change gamma. Log out and pick the X11 session on the login screen (**Plasma (X11)** or **GNOME on Xorg**). |

Some newer distros no longer ship an X11 session for GNOME or KDE. On those, gamma can't be
changed for now.

<details>
<summary><b>X11: install xrandr</b> (most desktops already have it)</summary>
<br>

```sh
sudo apt install x11-xserver-utils   # Debian, Ubuntu, Mint, Pop!_OS
sudo pacman -S xorg-xrandr           # Arch, Manjaro, EndeavourOS
sudo dnf install xrandr              # Fedora
sudo zypper install xrandr           # openSUSE
```

</details>

<details>
<summary><b>Sway / Hyprland: run wl-gammarelay-rs</b></summary>
<br>

Install it from your distro's packages if available (e.g. `wl-gammarelay-rs` on the AUR), or with Rust:

```sh
cargo install wl-gammarelay-rs --locked
```

Then start it with your session:

```sh
# Sway: ~/.config/sway/config
exec wl-gammarelay-rs

# Hyprland: ~/.config/hypr/hyprland.conf
exec-once = wl-gammarelay-rs
```

Only one gamma program can run at a time, so stop gammastep, wlsunset or hyprsunset while using it.

</details>

<details>
<summary><b>3. Optional: add it to your app menu</b></summary>
<br>

```sh
mkdir -p ~/.local/bin ~/.local/share/applications ~/.local/share/icons
mv GammaTool-linux-x64 ~/.local/bin/osu-gamma
curl -L -o ~/.local/share/icons/osu-gamma.png https://raw.githubusercontent.com/AmarilloNL/osu-gamma/main/icon.png
cat > ~/.local/share/applications/osu-gamma.desktop <<EOF
[Desktop Entry]
Name=Gamma Tool
Comment=Neon gamma booster for osu!
Exec=$HOME/.local/bin/osu-gamma
Icon=$HOME/.local/share/icons/osu-gamma.png
Type=Application
Categories=Utility;
EOF
```

</details>

<a id="build"></a>

## 🛠️ Build

<details>
<summary><b>Build commands</b></summary>
<br>

Windows (WPF, .NET Framework 4.8):

```
dotnet build GammaTool.csproj -c Release -o dist
```

Linux (Avalonia, in `linux/`):

```
dotnet publish linux/GammaTool.Linux.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o release/linux
```

The icon is generated with `powershell -File make-icon.ps1 -OutIco icon.ico -OutPreview icon.png`.

</details>
