using BandKeeper.Core.Models;

namespace BandKeeper.Core.Abstractions;

public interface ISampleRepository
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task SaveSampleAsync(NetworkSample sample, CancellationToken cancellationToken);
}
