namespace BandKeeper.Widget.Services;

public static class ThroughputFormatter
{
    private static readonly string[] Units = ["B/s", "KB/s", "MB/s", "GB/s", "TB/s"];

    public static string Format(double bytesPerSecond)
    {
        var value = Math.Max(0, bytesPerSecond);
        var unitIndex = 0;

        while (value >= 1024 && unitIndex < Units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:0.##} {Units[unitIndex]}";
    }
}
