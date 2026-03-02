using BandKeeper.Core.Models;

namespace BandKeeper.Core.Abstractions;

public interface INetworkCollector
{
    string CollectorName { get; }
    IAsyncEnumerable<NetworkSample> StreamSamplesAsync(CollectorSettings settings, CancellationToken cancellationToken);
}
