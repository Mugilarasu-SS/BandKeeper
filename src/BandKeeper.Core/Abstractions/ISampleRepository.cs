using BandKeeper.Core.Models;

namespace BandKeeper.Core.Abstractions;

public interface ISampleRepository
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task SaveSampleAsync(NetworkSample sample, CancellationToken cancellationToken);
    Task SaveWidgetSettingsAsync(WidgetSettings settings, CancellationToken cancellationToken);
    Task<WidgetSettings> GetWidgetSettingsAsync(CancellationToken cancellationToken);
    Task<UsageTotals> GetUsageTotalsAsync(DateTimeOffset startUtc, DateTimeOffset endUtc, string? adapterId, CancellationToken cancellationToken);
}
