using Microsoft.Win32;

namespace BandKeeper.Widget.Services;

public static class StartupRegistrationService
{
    private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string AppName = "BandKeeper";

    public static void SetStartWithWindows(bool enabled, string executablePath)
    {
        using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                           ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (runKey is null)
        {
            return;
        }

        if (enabled)
        {
            runKey.SetValue(AppName, $"\"{executablePath}\"");
        }
        else
        {
            runKey.DeleteValue(AppName, false);
        }
    }
}
