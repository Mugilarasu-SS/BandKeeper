namespace BandKeeper.Core.Models;

public sealed record UsageTotals(long DownloadBytes, long UploadBytes)
{
    public long TotalBytes => DownloadBytes + UploadBytes;
}
