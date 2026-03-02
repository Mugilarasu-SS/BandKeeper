namespace BandKeeper.Core.Models;

public sealed record NetworkSample(
    DateTimeOffset Timestamp,
    string AdapterId,
    string AdapterName,
    long RxBytes,
    long TxBytes,
    int? ProcessId = null,
    string? ProcessName = null);
