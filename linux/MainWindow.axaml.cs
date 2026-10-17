using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace GammaTool;

public partial class MainWindow : Window
{
    static readonly double[] PresetValues = { 1.00, 1.15, 1.30, 1.50, 1.80 };

    static string Fmt(double g) => g.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    double _target = 1;
    int _busy;

    public MainWindow()
    {
        InitializeComponent();
        foreach (var p in PresetValues)
        {
            var b = new RadioButton { Content = Fmt(p), Tag = p, Margin = new Thickness(3, 0) };
            b.Classes.Add("chip");
            b.Click += (_, _) => GammaSlider.Value = p;
            Presets.Children.Add(b);
        }
        GammaSlider.PropertyChanged += (_, e) => { if (e.Property == RangeBase.ValueProperty) Update(); };
        GammaSlider.Value = 1.0;
        Update();
    }

    void Update()
    {
        double g = Math.Round(GammaSlider.Value, 2);
        ValueText.Text = Fmt(g);
        foreach (RadioButton b in Presets.Children)
            b.IsChecked = (double)b.Tag! == g;
        Push(g);
    }

    // Applying spawns xrandr/busctl, so run it off the UI thread and only ever apply the latest value.
    void Push(double g)
    {
        _target = g;
        if (Interlocked.Exchange(ref _busy, 1) == 1) return;
        Task.Run(() =>
        {
            double done;
            string? error;
            do { done = _target; error = Gamma.Apply(done); } while (done != _target);
            Volatile.Write(ref _busy, 0);
            Dispatcher.UIThread.Post(() => Status.Text = error ?? "");
            if (done != _target) Push(_target);
        });
    }

    void OnReset(object? sender, RoutedEventArgs e) => GammaSlider.Value = 1.0;
    void OnMinimize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    void OnClose(object? sender, RoutedEventArgs e) => Close();

    void OnDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _target = 1;
        SpinWait.SpinUntil(() => Volatile.Read(ref _busy) == 0, 2000);
        Gamma.Apply(1);
        base.OnClosed(e);
    }
}
