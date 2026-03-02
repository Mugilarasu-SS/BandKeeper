namespace BandKeeper.Core.Models;

public sealed class WidgetSettings
{
    public string AdapterId { get; init; } = "auto";
    public bool IncludeVpnAndTunnelAdapters { get; init; }
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);
    public bool StartWithWindows { get; init; }
    public double OpacityPercent { get; init; } = 70;
}
