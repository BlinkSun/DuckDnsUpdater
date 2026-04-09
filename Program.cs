namespace DuckDnsUpdater;

internal static class Program
{
    private static readonly string FatalLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DuckDnsUpdater",
        "duckdns-updater.log");

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            LogFatal(ex, "Fatal crash in application startup.");
            MessageBox.Show(
                "DuckDNS Updater encountered a fatal error and must close.",
                "DuckDNS Updater",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void OnThreadException(object? sender, ThreadExceptionEventArgs e)
    {
        LogFatal(e.Exception, "Unhandled UI thread exception.");
        MessageBox.Show(
            "DuckDNS Updater hit an unexpected UI error. The event was handled.",
            "DuckDNS Updater",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        if (ex != null)
        {
            LogFatal(ex, "Unhandled AppDomain exception.");
        }
        else
        {
            LogFatal(null, "Unhandled AppDomain exception with non-exception payload.");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogFatal(e.Exception, "Unobserved task exception.");
        e.SetObserved();
    }

    private static void LogFatal(Exception? ex, string context)
    {
        try
        {
            string? directory = Path.GetDirectoryName(FatalLogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string detail = ex == null ? context : $"{context} {ex}";
            string line = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] [FATAL] {detail}{Environment.NewLine}";
            File.AppendAllText(FatalLogPath, line);
        }
        catch
        {
            // Never throw from the global exception handlers.
        }
    }
}
