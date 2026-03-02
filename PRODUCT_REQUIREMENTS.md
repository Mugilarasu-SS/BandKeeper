# BandKeeper Product Requirements (v1)

## 1) Product Vision
BandKeeper is a lightweight, transparent Windows network monitor for versatile users (individual and enterprise contexts) that provides:
- Real-time uplink/downlink visibility.
- Accurate internet usage accounting over configurable periods.
- A non-intrusive always-on desktop widget.

## 2) Product Goals
Priority order:
1. Beautiful always-on widget.
2. Highly accurate bandwidth and usage accounting.
3. Minimal CPU and RAM usage.

Target performance budget:
- CPU: under 1–2% on normal operation.
- RAM: under 50 MB.

## 3) Platform and Distribution
- OS support:
  - Windows 11 (all versions).
  - Windows Server (supported modern editions).
- Distribution:
  - Installer-based delivery (.msi or .exe).
- Licensing:
  - Open-source.
- Deployment mode:
  - Team/deployable enterprise-friendly packaging and configuration.

## 4) Core Functional Requirements

### 4.1 Real-Time Monitoring
- User-selectable update interval (e.g., 250 ms, 500 ms, 1 s, 2 s).
- User-selected network adapter monitoring.
- Auto-scaled units for throughput display.
- Real-time display includes:
  - Current download speed.
  - Current upload speed.

### 4.2 Interface Scope
- Must support optional inclusion/exclusion of VPN/tunnel interfaces (user-configurable).

### 4.3 Usage Tracking and Timeframes
- Data collection source preference: packet-level capture for accuracy (with documented overhead/permissions).
- Usage views:
  - Hour
  - Day
  - Week
  - Month
  - Year
  - Custom range
- Custom range input: calendar date-time picker.
- Historical reporting must include:
  - Upload/download split.
  - Total usage.
  - Per-app/process split (e.g., browser, games, streaming apps).

### 4.4 Data Retention
- Retention should be user-configurable.
- Provide options from rolling retention windows to “keep forever”.

## 5) UI/UX Requirements

### 5.1 Visual Behavior
- Transparent desktop overlay widget.
- Adjustable opacity slider in the main app settings (not inside the widget itself).
- Auto-hide when fullscreen app/game is active.
- Minimal mode showing only numeric speeds in this format: `DL: xx unit/s  UL: xx unit/s`.

### 5.2 Interaction Behavior
- Click-through mode (widget does not block mouse interaction behind it).
- Triple-click on widget terminates the widget process.
- Startup behavior:
  - Optional start with Windows.
  - Otherwise start only when user launches it.

### 5.3 Display and Personalization
- Remember per-monitor layout and position.
- Accent color customization.
- Keep desktop disturbance minimal.

## 6) Alerts and Intelligence
- Data cap management with monthly quota support.
- Optional user-configured warning thresholds at 80%, 95%, 100% of quota.
- Alert channels:
  - Desktop toast notifications.
  - Sound alerts.
- Detect and notify sudden background data spikes.
- Daily and weekly summaries:
  - In-app reporting.
  - Export-capable output.

## 7) Privacy and Data Handling
- Local-only data handling by default.
- Continue collecting local counters when internet is unavailable.
- If no internet connection is detected, show a professional offline status prompt/indicator.
- In offline state, keep the app functional while making the live internet-speed widget unavailable until connectivity returns.
- Crash reporting is optional and user-controlled.
- Export/import support:
  - CSV
  - JSON

## 8) Recommended Technical Architecture

### 8.1 Suggested Stack
Given the performance, UX, and Windows-only targets:
- Runtime/UI: .NET 8 with WinUI 3 (or WPF fallback if lower complexity is required initially).
- Monitoring engine:
  - Pluggable collectors:
    - Adapter counters collector.
    - Packet capture collector (Npcap/Windows APIs) for higher-fidelity accounting.
    - App/process usage attribution pipeline to support app-wise data usage reports.
- Storage:
  - SQLite for local time-series + usage aggregates.
- Packaging:
  - MSI/EXE installer and optional enterprise deployment presets.

### 8.2 Process Model
- Split architecture:
  - Background service/agent for collection and aggregation.
  - Foreground widget UI process for rendering and interaction.
- Benefits:
  - Better UI responsiveness.
  - Safer auto-restart and crash isolation.
  - Enterprise deployability.

### 8.3 Data Model (High-Level)
- Samples table: timestamp, adapter, rx_bytes, tx_bytes, process_id (when available).
- Aggregates table: period bucket, totals, upload, download.
- Settings table: UI prefs, adapter selection, thresholds, retention, startup.

## 9) Non-Functional Requirements
- App should remain responsive under heavy traffic conditions.
- Startup time target: < 2 seconds for widget availability.
- Graceful handling for sleep/resume, adapter reset, VPN connect/disconnect.
- No blocking UI operations on sampling/aggregation path.

## 10) Milestone Plan

### Milestone 1: Foundation (MVP)
- Transparent widget with real-time up/down.
- User-selectable adapter and refresh interval.
- Hour/day/week/month/year/custom totals.
- SQLite persistence and basic settings.
- Startup option and tray integration.

### Milestone 2: Premium Usability
- Opacity, click-through, fullscreen auto-hide.
- Per-monitor layout memory.
- Alerts (toast/sound), quota thresholds.
- Daily/weekly summaries and CSV/JSON export.

### Milestone 3: Accuracy + Advanced Insights
- Packet capture mode for high-fidelity accounting.
- Per-process/app usage analytics.
- Spike detection and anomaly signal tuning.
- Enterprise deployment docs and policy templates.

## 11) Open Decisions to Finalize Before Build
1. Packet capture library choice and driver strategy for enterprise environments.
2. Minimum supported Windows Server editions.
3. Final decision: WinUI 3 vs WPF for MVP timeline risk.
4. Code signing policy for release channels.
