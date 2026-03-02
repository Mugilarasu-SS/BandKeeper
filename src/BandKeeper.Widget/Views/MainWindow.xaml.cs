using System.Windows;
using BandKeeper.Core.Models;
using BandKeeper.Infrastructure.Data;
using BandKeeper.Infrastructure.Monitoring;
using BandKeeper.Widget.Services;
using BandKeeper.Widget.ViewModels;

namespace BandKeeper.Widget.Views;

public partial class MainWindow : Window
{
    private readonly WidgetViewModel _viewModel = new();
    private readonly AdapterCounterCollector _collector = new();
    private readonly NetworkRateCalculator _rateCalculator = new();
    private readonly SqliteSampleRepository _repository;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _monitoringTask;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databasePath = Path.Combine(appData, "BandKeeper", "bandkeeper.db");
        _repository = new SqliteSampleRepository(databasePath);

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _repository.InitializeAsync(_cancellationTokenSource.Token);

            var widgetSettings = await _repository.GetWidgetSettingsAsync(_cancellationTokenSource.Token);
            await _repository.SaveWidgetSettingsAsync(widgetSettings, _cancellationTokenSource.Token);

            var collectorSettings = new CollectorSettings
            {
                AdapterId = widgetSettings.AdapterId,
                IncludeVpnAndTunnelAdapters = widgetSettings.IncludeVpnAndTunnelAdapters,
                PollingInterval = widgetSettings.PollingInterval
            };

            _monitoringTask = Task.Run(() => StartMonitoringAsync(collectorSettings, _cancellationTokenSource.Token));
        }
        catch (Exception)
        {
            _viewModel.DownloadText = "DL: unavailable";
            _viewModel.UploadText = "UL: unavailable";
        }
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

    private async void OnClosed(object? sender, EventArgs e)
    {
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
