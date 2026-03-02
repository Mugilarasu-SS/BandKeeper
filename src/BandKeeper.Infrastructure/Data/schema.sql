PRAGMA journal_mode = WAL;

CREATE TABLE IF NOT EXISTS samples (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp_utc TEXT NOT NULL,
    adapter_id TEXT NOT NULL,
    adapter_name TEXT NOT NULL,
    rx_bytes INTEGER NOT NULL,
    tx_bytes INTEGER NOT NULL,
    process_id INTEGER NULL,
    process_name TEXT NULL
);

CREATE INDEX IF NOT EXISTS ix_samples_timestamp ON samples(timestamp_utc);
CREATE INDEX IF NOT EXISTS ix_samples_adapter ON samples(adapter_id);

CREATE TABLE IF NOT EXISTS aggregates (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    period_type TEXT NOT NULL,
    period_start_utc TEXT NOT NULL,
    period_end_utc TEXT NOT NULL,
    adapter_id TEXT NOT NULL,
    total_bytes INTEGER NOT NULL,
    upload_bytes INTEGER NOT NULL,
    download_bytes INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_aggregates_period ON aggregates(period_type, period_start_utc, period_end_utc);

CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);
