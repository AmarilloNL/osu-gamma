using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GammaTool;

public partial class MainWindow : Window
{
    static readonly double[] PresetValues = { 1.00, 1.15, 1.30, 1.50, 1.80 };
    static readonly CultureInfo Comma = new("nl-NL");

    static string Fmt(double g) => g.ToString("0.00", Comma);

    public MainWindow()
    {
        InitializeComponent();
        foreach (var p in PresetValues)
        {
            var b = new RadioButton { Content = Fmt(p), Tag = p, Margin = new Thickness(3, 0, 3, 0), Style = (Style)FindResource("Preset") };
            b.Click += (_, _) => GammaSlider.Value = p;
            Presets.Children.Add(b);
        }
        GammaSlider.ValueChanged += (_, _) => Update();
        GammaSlider.Value = 1.0;
        Update();
    }

    void Update()
    {
        double g = Math.Round(GammaSlider.Value, 2);
        ValueText.Text = Fmt(g);
        foreach (RadioButton b in Presets.Children)
            b.IsChecked = (double)b.Tag == g;

        bool ok = Gamma.Apply(g);
        bool unlocked = !ok && Gamma.IsUnlocked;
        Status.Text = ok ? "" : unlocked ? "windows blocked this value. sign out and back in to finish unlocking" : "windows blocked this value";
        UnlockButton.Visibility = ok || unlocked ? Visibility.Collapsed : Visibility.Visible;
    }

    void OnUnlock(object sender, RoutedEventArgs e)
    {
        if (Gamma.Unlock())
        {
            UnlockButton.Visibility = Visibility.Collapsed;
            Status.Text = "unlocked. sign out and back in, then try again";
        }
    }

    void OnReset(object sender, RoutedEventArgs e) => GammaSlider.Value = 1.0;
    void OnDrag(object sender, MouseButtonEventArgs e) => DragMove();
    void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    void OnClose(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        Gamma.Apply(1.0);
        base.OnClosed(e);
    }
}
