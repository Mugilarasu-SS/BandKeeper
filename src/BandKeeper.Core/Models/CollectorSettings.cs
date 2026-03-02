namespace BandKeeper.Core.Models;

public sealed class CollectorSettings
{
    public required string AdapterId { get; init; }
    public bool IncludeVpnAndTunnelAdapters { get; init; }
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);
}
