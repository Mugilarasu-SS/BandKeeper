using BandKeeper.Core.Abstractions;
using BandKeeper.Core.Models;

namespace BandKeeper.Infrastructure.Monitoring;

public sealed class NetworkRateCalculator : INetworkRateCalculator
{
    public NetworkRate Calculate(NetworkSample previous, NetworkSample current)
    {
        var seconds = (current.Timestamp - previous.Timestamp).TotalSeconds;
        if (seconds <= 0)
        {
            return new NetworkRate(current.Timestamp, current.AdapterId, 0, 0);
        }

        var download = Math.Max(0, current.RxBytes - previous.RxBytes) / seconds;
        var upload = Math.Max(0, current.TxBytes - previous.TxBytes) / seconds;

        return new NetworkRate(current.Timestamp, current.AdapterId, download, upload);
    }
}
