using System.Reflection;
using BandKeeper.Core.Abstractions;
using BandKeeper.Core.Models;
using Microsoft.Data.Sqlite;

namespace BandKeeper.Infrastructure.Data;

public sealed class SqliteSampleRepository : ISampleRepository
{
    private const string AdapterIdKey = "widget.adapterId";
    private const string IncludeVpnKey = "widget.includeVpnAndTunnelAdapters";
    private const string PollingIntervalMsKey = "widget.pollingIntervalMs";
    private const string StartWithWindowsKey = "widget.startWithWindows";
    private const string OpacityPercentKey = "widget.opacityPercent";

    private readonly string _databasePath;

    public SqliteSampleRepository(string databasePath)
    {
        _databasePath = databasePath;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);

        var schema = await File.ReadAllTextAsync(ResolveSchemaPath(), cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveSampleAsync(NetworkSample sample, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO samples(
    timestamp_utc,
    adapter_id,
    adapter_name,
    rx_bytes,
    tx_bytes,
    process_id,
    process_name)
VALUES (
    $timestampUtc,
    $adapterId,
    $adapterName,
    $rxBytes,
    $txBytes,
    $processId,
    $processName);";

        command.Parameters.AddWithValue("$timestampUtc", sample.Timestamp.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$adapterId", sample.AdapterId);
        command.Parameters.AddWithValue("$adapterName", sample.AdapterName);
        command.Parameters.AddWithValue("$rxBytes", sample.RxBytes);
        command.Parameters.AddWithValue("$txBytes", sample.TxBytes);
        command.Parameters.AddWithValue("$processId", (object?)sample.ProcessId ?? DBNull.Value);
        command.Parameters.AddWithValue("$processName", (object?)sample.ProcessName ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveWidgetSettingsAsync(WidgetSettings settings, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await UpsertSettingAsync(connection, AdapterIdKey, settings.AdapterId, cancellationToken);
        await UpsertSettingAsync(connection, IncludeVpnKey, settings.IncludeVpnAndTunnelAdapters ? "1" : "0", cancellationToken);
        await UpsertSettingAsync(connection, PollingIntervalMsKey, ((int)settings.PollingInterval.TotalMilliseconds).ToString(), cancellationToken);
        await UpsertSettingAsync(connection, StartWithWindowsKey, settings.StartWithWindows ? "1" : "0", cancellationToken);
        await UpsertSettingAsync(connection, OpacityPercentKey, settings.OpacityPercent.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken);
    }

    public async Task<WidgetSettings> GetWidgetSettingsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var adapterId = await GetSettingAsync(connection, AdapterIdKey, cancellationToken) ?? "auto";
        var includeVpnValue = await GetSettingAsync(connection, IncludeVpnKey, cancellationToken) ?? "0";
        var pollingMsValue = await GetSettingAsync(connection, PollingIntervalMsKey, cancellationToken) ?? "1000";
        var startWithWindowsValue = await GetSettingAsync(connection, StartWithWindowsKey, cancellationToken) ?? "0";
        var opacityPercentValue = await GetSettingAsync(connection, OpacityPercentKey, cancellationToken) ?? "70";

        var includeVpn = includeVpnValue == "1";
        var startWithWindows = startWithWindowsValue == "1";
        var pollingMs = int.TryParse(pollingMsValue, out var parsedMs) ? parsedMs : 1000;
        var opacityPercent = double.TryParse(opacityPercentValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedOpacity) ? parsedOpacity : 70;

        return new WidgetSettings
        {
            AdapterId = adapterId,
            IncludeVpnAndTunnelAdapters = includeVpn,
            PollingInterval = TimeSpan.FromMilliseconds(Math.Max(250, pollingMs)),
            StartWithWindows = startWithWindows,
            OpacityPercent = Math.Clamp(opacityPercent, 20, 100)
        };
    }

    public async Task<UsageTotals> GetUsageTotalsAsync(DateTimeOffset startUtc, DateTimeOffset endUtc, string? adapterId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    MIN(rx_bytes) AS min_rx,
    MAX(rx_bytes) AS max_rx,
    MIN(tx_bytes) AS min_tx,
    MAX(tx_bytes) AS max_tx
FROM samples
WHERE timestamp_utc >= $startUtc
  AND timestamp_utc <= $endUtc
  AND ($adapterId IS NULL OR adapter_id = $adapterId);";

        command.Parameters.AddWithValue("$startUtc", startUtc.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$endUtc", endUtc.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$adapterId", (object?)adapterId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new UsageTotals(0, 0);
        }

        var minRx = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
        var maxRx = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        var minTx = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
        var maxTx = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);

        return new UsageTotals(
            DownloadBytes: Math.Max(0, maxRx - minRx),
            UploadBytes: Math.Max(0, maxTx - minTx));
    }

    public string GetDatabasePath() => _databasePath;

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task UpsertSettingAsync(SqliteConnection connection, string key, string value, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO settings(key, value)
VALUES ($key, $value)
ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
        command.Parameters.AddWithValue("$key", key);
        command.Parameters.AddWithValue("$value", value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string?> GetSettingAsync(SqliteConnection connection, string key, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM settings WHERE key = $key LIMIT 1;";
        command.Parameters.AddWithValue("$key", key);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result?.ToString();
    }

    private static string ResolveSchemaPath()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                ?? AppContext.BaseDirectory;
        var schemaPath = Path.Combine(assemblyDirectory, "Data", "schema.sql");

        if (File.Exists(schemaPath))
        {
            return schemaPath;
        }

        throw new FileNotFoundException("Could not locate SQLite schema file.", schemaPath);
    }
}
