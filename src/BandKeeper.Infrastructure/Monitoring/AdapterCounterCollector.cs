using System.Net.NetworkInformation;
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
        while (!cancellationToken.IsCancellationRequested)
        {
            var adapter = ResolveAdapter(settings);
            if (adapter is not null)
            {
                var stats = adapter.GetIPStatistics();

                yield return new NetworkSample(
                    DateTimeOffset.UtcNow,
                    adapter.Id,
                    adapter.Name,
                    stats.BytesReceived,
                    stats.BytesSent);
            }

            await Task.Delay(settings.PollingInterval, cancellationToken);
        }
    }

    public static IReadOnlyList<AdapterInfo> GetAvailableAdapters(bool includeVpnAndTunnelAdapters)
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up)
            .Where(i => includeVpnAndTunnelAdapters || !IsVpnOrTunnel(i))
            .Select(i => new AdapterInfo(i.Id, i.Name))
            .OrderBy(i => i.Name)
            .ToList();
    }

    private static NetworkInterface? ResolveAdapter(CollectorSettings settings)
    {
        var adapters = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up)
            .Where(i => settings.IncludeVpnAndTunnelAdapters || !IsVpnOrTunnel(i))
            .ToList();

        if (adapters.Count == 0)
        {
            return null;
        }

        var selected = adapters.FirstOrDefault(i =>
            string.Equals(i.Id, settings.AdapterId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(i.Name, settings.AdapterId, StringComparison.OrdinalIgnoreCase));

        return selected ?? adapters[0];
    }

    private static bool IsVpnOrTunnel(NetworkInterface networkInterface)
    {
        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
        {
            return true;
        }

        var name = networkInterface.Name;
        var description = networkInterface.Description;

        return name.Contains("vpn", StringComparison.OrdinalIgnoreCase)
               || description.Contains("vpn", StringComparison.OrdinalIgnoreCase)
               || description.Contains("wireguard", StringComparison.OrdinalIgnoreCase)
               || description.Contains("openvpn", StringComparison.OrdinalIgnoreCase)
               || description.Contains("tap", StringComparison.OrdinalIgnoreCase)
               || description.Contains("tun", StringComparison.OrdinalIgnoreCase);
    }
}
