using BandKeeper.Core.Abstractions;
using BandKeeper.Core.Models;

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
        // Milestone 1 scaffold:
        // - Ensure DB file path exists
        // - Execute schema.sql statements
        await Task.CompletedTask;
    }

    public async Task SaveSampleAsync(NetworkSample sample, CancellationToken cancellationToken)
    {
        // Milestone 1 scaffold:
        // Persist sample rows into SQLite.
        await Task.CompletedTask;
    }

    public string GetDatabasePath() => _databasePath;
}
