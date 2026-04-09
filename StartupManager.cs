using Microsoft.Win32;

namespace DuckDnsUpdater;

internal static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "DuckDnsUpdater";

    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        string? configuredCommand = key?.GetValue(AppName) as string;
        if (string.IsNullOrWhiteSpace(configuredCommand))
        {
            return false;
        }

        return string.Equals(configuredCommand, BuildCommand(), StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath)
            ?? throw new InvalidOperationException("Unable to open startup registry key.");

        if (enabled)
        {
            key.SetValue(AppName, BuildCommand(), RegistryValueKind.String);
            return;
        }

        if (key.GetValue(AppName) != null)
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }

    private static string BuildCommand()
    {
        string exePath = Application.ExecutablePath;
        return $"\"{exePath}\"";
    }
}
