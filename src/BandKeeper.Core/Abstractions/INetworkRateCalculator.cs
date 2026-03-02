using BandKeeper.Core.Models;

namespace BandKeeper.Core.Abstractions;

public interface INetworkRateCalculator
{
    NetworkRate Calculate(NetworkSample previous, NetworkSample current);
}
