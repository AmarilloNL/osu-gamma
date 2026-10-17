using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace GammaTool;

static class Gamma
{
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] static extern IntPtr CreateDC(string? driver, string device, string? output, IntPtr initData);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] static extern bool SetDeviceGammaRamp(IntPtr hdc, ushort[] ramp);

    const string IcmKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ICM";

    // Returns false if Windows rejected the ramp on any monitor (outside its allowed range).
    public static bool Apply(double gamma)
    {
        var ramp = new ushort[3 * 256];
        for (int i = 0; i < 256; i++)
            ramp[i] = ramp[256 + i] = ramp[512 + i] = (ushort)Math.Round(Math.Pow(i / 255.0, 1 / gamma) * 65535);

        bool ok = true;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var hdc = CreateDC(null, screen.DeviceName, null, IntPtr.Zero);
            ok &= SetDeviceGammaRamp(hdc, ramp);
            DeleteDC(hdc);
        }
        return ok;
    }

    // GdiIcmGammaRange=256 lifts Windows' gamma clamp; takes effect after sign-out.
    public static bool IsUnlocked =>
        Registry.LocalMachine.OpenSubKey(IcmKey)?.GetValue("GdiIcmGammaRange") is int v && v == 256;

    public static bool Unlock()
    {
        try
        {
            Process.Start(new ProcessStartInfo("reg.exe", $"add \"HKLM\\{IcmKey}\" /v GdiIcmGammaRange /t REG_DWORD /d 256 /f")
            {
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            })!.WaitForExit();
        }
        catch (Win32Exception) { } // UAC prompt cancelled
        return IsUnlocked;
    }
}
