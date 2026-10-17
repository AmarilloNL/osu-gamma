using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace GammaTool;

static class Gamma
{
    // Returns null on success, otherwise what the player should do about it.
    public static string? Apply(double gamma)
    {
        if (Env("XDG_SESSION_TYPE") == "wayland" || Env("WAYLAND_DISPLAY") != "")
        {
            // wl-gammarelay-rs ramps x^gamma (above 1 darkens), the opposite of xrandr and Windows
            if (Run("busctl", $"--user set-property rs.wl-gammarelay / rs.wl.gammarelay Gamma d {Num(1 / gamma)}", out _))
                return null;
            var desktop = Env("XDG_CURRENT_DESKTOP").ToUpperInvariant();
            return desktop.Contains("KDE") || desktop.Contains("GNOME")
                ? "kde/gnome on wayland blocks gamma. log in with an x11 session"
                : "start wl-gammarelay-rs first, or log in with an x11 session";
        }

        if (!Run("xrandr", "--listactivemonitors", out var monitors))
            return "xrandr not found. install it (x11-xserver-utils / xorg-xrandr)";

        // " 0: +*HDMI-1 1920/531x1080/299+0+0  HDMI-1" -> output name is the last field
        foreach (var line in monitors.Split('\n').Skip(1))
        {
            var output = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (output == null) continue;
            var g = Num(gamma);
            if (!Run("xrandr", $"--output {output} --gamma {g}:{g}:{g}", out _))
                return $"xrandr couldn't set gamma on {output}";
        }
        return null;
    }

    static string Env(string name) => Environment.GetEnvironmentVariable(name) ?? "";
    static string Num(double x) => x.ToString("0.####", CultureInfo.InvariantCulture);

    static bool Run(string file, string args, out string stdout)
    {
        stdout = "";
        try
        {
            using var p = Process.Start(new ProcessStartInfo(file, args) { RedirectStandardOutput = true, UseShellExecute = false })!;
            stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch (Win32Exception) { return false; } // not installed
    }
}
