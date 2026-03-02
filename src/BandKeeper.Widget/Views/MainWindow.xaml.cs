using System.Drawing;
using System.Reflection;
using System.Windows;
using BandKeeper.Core.Models;
using BandKeeper.Infrastructure.Data;
using BandKeeper.Infrastructure.Monitoring;
using BandKeeper.Widget.Models;
using BandKeeper.Widget.Services;
using BandKeeper.Widget.ViewModels;
using Forms = System.Windows.Forms;

namespace BandKeeper.Widget.Views;

public partial class MainWindow : Window
{
    private readonly WidgetViewModel _viewModel = new();
    private readonly AdapterCounterCollector _collector = new();
    private readonly NetworkRateCalculator _rateCalculator = new();
    private readonly SqliteSampleRepository _repository;

    private WidgetSettings _widgetSettings = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _monitoringTask;
    private Forms.NotifyIcon? _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databasePath = Path.Combine(appData, "BandKeeper", "bandkeeper.db");
        _repository = new SqliteSampleRepository(databasePath);

        Loaded += OnLoaded;
        Closed += OnClosed;
        MouseLeftButtonDown += (_, _) => DragMove();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _repository.InitializeAsync(_cancellationTokenSource.Token);
            _widgetSettings = await _repository.GetWidgetSettingsAsync(_cancellationTokenSource.Token);
            await _repository.SaveWidgetSettingsAsync(_widgetSettings, _cancellationTokenSource.Token);

            ApplyWindowSettings(_widgetSettings);
            StartupRegistrationService.SetStartWithWindows(_widgetSettings.StartWithWindows, GetExecutablePath());
            SetupTrayIcon();

            StartMonitoring(_widgetSettings);
        }
        catch (Exception)
        {
            _viewModel.DownloadText = "DL: unavailable";
            _viewModel.UploadText = "UL: unavailable";
        }
    }

    private void StartMonitoring(WidgetSettings settings)
    {
        if (_cancellationTokenSource is null)
        {
            return;
        }

        var collectorSettings = new CollectorSettings
        {
            AdapterId = settings.AdapterId,
            IncludeVpnAndTunnelAdapters = settings.IncludeVpnAndTunnelAdapters,
            PollingInterval = settings.PollingInterval
        };

        _monitoringTask = Task.Run(() => StartMonitoringAsync(collectorSettings, _cancellationTokenSource.Token));
    }

    private async Task StartMonitoringAsync(CollectorSettings settings, CancellationToken cancellationToken)
    {
        NetworkSample? previous = null;

        await foreach (var sample in _collector.StreamSamplesAsync(settings, cancellationToken).ConfigureAwait(false))
        {
            await _repository.SaveSampleAsync(sample, cancellationToken).ConfigureAwait(false);

            if (previous is not null)
            {
                var rate = _rateCalculator.Calculate(previous, sample);
                await Dispatcher.InvokeAsync(() =>
                {
                    _viewModel.DownloadText = $"DL: {ThroughputFormatter.Format(rate.DownloadBytesPerSecond)}";
                    _viewModel.UploadText = $"UL: {ThroughputFormatter.Format(rate.UploadBytesPerSecond)}";
                });
            }

            previous = sample;
        }
    }

    private void SetupTrayIcon()
    {
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open Settings", null, async (_, _) => await OpenSettingsAsync());
        contextMenu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(Close));

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(GetExecutablePath()) ?? SystemIcons.Application,
            Text = "BandKeeper",
            Visible = true,
            ContextMenuStrip = contextMenu
        };

        _notifyIcon.DoubleClick += (_, _) => Dispatcher.Invoke(async () => await OpenSettingsAsync());
    }

    private async Task OpenSettingsAsync()
    {
        var adapters = AdapterCounterCollector
            .GetAvailableAdapters(_widgetSettings.IncludeVpnAndTunnelAdapters)
            .Select(a => new AdapterOption(a.Id, a.Name))
            .ToList();

        var settingsWindow = new SettingsWindow(_widgetSettings, adapters)
        {
            Owner = this
        };

        var accepted = settingsWindow.ShowDialog() == true;
        if (!accepted || settingsWindow.SavedSettings is null || _cancellationTokenSource is null)
        {
            return;
        }

        _widgetSettings = settingsWindow.SavedSettings;
        ApplyWindowSettings(_widgetSettings);
        StartupRegistrationService.SetStartWithWindows(_widgetSettings.StartWithWindows, GetExecutablePath());

        await _repository.SaveWidgetSettingsAsync(_widgetSettings, _cancellationTokenSource.Token);

        _cancellationTokenSource.Cancel();
        if (_monitoringTask is not null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // expected on restart
            }
        }

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        StartMonitoring(_widgetSettings);
    }

    private void ApplyWindowSettings(WidgetSettings settings)
    {
        Opacity = settings.OpacityPercent / 100;
    }

    private static string GetExecutablePath()
    {
        return Assembly.GetEntryAssembly()?.Location ?? Environment.ProcessPath ?? string.Empty;
    }

    private async void OnClosed(object? sender, EventArgs e)
    {
        _notifyIcon?.Dispose();

        if (_cancellationTokenSource is null)
        {
            return;
        }

        _cancellationTokenSource.Cancel();

        if (_monitoringTask is not null)
        {
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
        }

        _cancellationTokenSource.Dispose();
    }
}
