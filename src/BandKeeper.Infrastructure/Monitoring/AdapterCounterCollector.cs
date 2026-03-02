using BandKeeper.Core.Abstractions;
using BandKeeper.Core.Models;

namespace BandKeeper.Infrastructure.Monitoring;

public sealed class AdapterCounterCollector : INetworkCollector
{
    public string CollectorName => "AdapterCounters";

    public async IAsyncEnumerable<NetworkSample> StreamSamplesAsync(
        CollectorSettings settings,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Milestone 1 scaffold:
        // A real implementation will query adapter counters from Windows APIs and yield real samples.
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new NetworkSample(
                DateTimeOffset.UtcNow,
                settings.AdapterId,
                "Scaffold Adapter",
                0,
                0);

            await Task.Delay(settings.PollingInterval, cancellationToken);
        }
    }
}
