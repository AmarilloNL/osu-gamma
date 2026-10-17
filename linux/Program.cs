using System;
using Avalonia;

namespace GammaTool;

static class Program
{
    [STAThread]
    static void Main(string[] args) =>
        AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime(args);
}
