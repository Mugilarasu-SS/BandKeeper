using System.Reflection;
using BandKeeper.Core.Abstractions;
using BandKeeper.Core.Models;
using Microsoft.Data.Sqlite;

namespace BandKeeper.Infrastructure.Data;

public sealed class SqliteSampleRepository : ISampleRepository
{
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

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var schema = await File.ReadAllTextAsync(ResolveSchemaPath(), cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveSampleAsync(NetworkSample sample, CancellationToken cancellationToken)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

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

    public string GetDatabasePath() => _databasePath;

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
