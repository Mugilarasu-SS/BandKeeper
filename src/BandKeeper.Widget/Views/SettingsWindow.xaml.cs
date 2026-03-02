using System.Windows;
using BandKeeper.Core.Models;
using BandKeeper.Widget.Models;

namespace BandKeeper.Widget.Views;

public partial class SettingsWindow : Window
{
    public WidgetSettings? SavedSettings { get; private set; }

    public SettingsWindow(WidgetSettings settings, IReadOnlyList<AdapterOption> adapters)
    {
        InitializeComponent();

        var options = new List<AdapterOption> { new("auto", "Auto (best active adapter)") };
        options.AddRange(adapters);

        AdapterComboBox.ItemsSource = options;
        AdapterComboBox.SelectedItem = options.FirstOrDefault(a => a.Id == settings.AdapterId) ?? options[0];

        IncludeVpnCheckBox.IsChecked = settings.IncludeVpnAndTunnelAdapters;
        PollingIntervalTextBox.Text = ((int)settings.PollingInterval.TotalMilliseconds).ToString();
        OpacityTextBox.Text = settings.OpacityPercent.ToString("0");
        StartWithWindowsCheckBox.IsChecked = settings.StartWithWindows;
    }

    private void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        var selectedAdapter = AdapterComboBox.SelectedItem as AdapterOption;
        var includeVpn = IncludeVpnCheckBox.IsChecked == true;
        var startWithWindows = StartWithWindowsCheckBox.IsChecked == true;

        if (!int.TryParse(PollingIntervalTextBox.Text, out var pollingMs))
        {
            MessageBox.Show(this, "Polling interval must be a number in milliseconds.", "Invalid value");
            return;
        }

        if (!double.TryParse(OpacityTextBox.Text, out var opacityPercent))
        {
            MessageBox.Show(this, "Opacity must be a number (20-100).", "Invalid value");
            return;
        }

        SavedSettings = new WidgetSettings
        {
            AdapterId = selectedAdapter?.Id ?? "auto",
            IncludeVpnAndTunnelAdapters = includeVpn,
            PollingInterval = TimeSpan.FromMilliseconds(Math.Clamp(pollingMs, 250, 5000)),
            StartWithWindows = startWithWindows,
            OpacityPercent = Math.Clamp(opacityPercent, 20, 100)
        };

        DialogResult = true;
        Close();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
