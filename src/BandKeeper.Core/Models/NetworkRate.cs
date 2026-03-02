namespace BandKeeper.Core.Models;

public sealed record NetworkRate(
    DateTimeOffset Timestamp,
    string AdapterId,
    double DownloadBytesPerSecond,
    double UploadBytesPerSecond);
