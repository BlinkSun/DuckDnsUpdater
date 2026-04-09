using DuckDnsUpdater.Core;

namespace DuckDnsUpdater;

public class MainForm : Form
{
    private enum HealthState
    {
        Ok,
        Warning,
        Error
    }

    private static readonly HttpClient SharedHttpClient = CreateHttpClient();
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1500)];
    private static readonly string[] PublicIpProviders =
    [
        "https://checkip.amazonaws.com",
        "https://api.ipify.org"
    ];
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DuckDnsUpdater",
        "duckdns-updater.log");
    private static readonly string StatePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DuckDnsUpdater",
        "duckdns-updater.state");

    private NotifyIcon? trayIcon;
    private ContextMenuStrip? trayMenu;
    private System.Windows.Forms.Timer? timer;
    private ToolStripMenuItem? statusMenuItem;
    private ToolStripMenuItem? lastCheckMenuItem;
    private ToolStripMenuItem? lastSuccessMenuItem;
    private ToolStripMenuItem? lastIpMenuItem;
    private ToolStripMenuItem? lastErrorMenuItem;
    private ToolStripMenuItem? nextCheckMenuItem;
    private string OldIp = string.Empty;
    private string lastKnownIp = "N/A";
    private string lastError = "None";
    private DateTimeOffset? lastCheckAt;
    private DateTimeOffset? lastSuccessAt;
    private DateTimeOffset nextCheckAt;
    private HealthState healthState = HealthState.Warning;
    private bool hasPendingRecoveryNotification;
    private string lastSilentFailureSignature = string.Empty;

    public MainForm()
    {
        Name = "MainForm";
        WindowState = FormWindowState.Minimized;
        Text = "DuckDNS Updater";
        ShowInTaskbar = false;

        InitializeTrayMenu();
        InitializeTrayIcon();
        LoadRuntimeState();
        InitializeTimer();
    }

    private void InitializeTrayMenu()
    {
        trayMenu = new ContextMenuStrip();
        trayMenu.Opening += OnTrayMenuOpening;
        statusMenuItem = new ToolStripMenuItem("Status: Starting...") { Enabled = false };
        lastCheckMenuItem = new ToolStripMenuItem("Last check: N/A") { Enabled = false };
        lastSuccessMenuItem = new ToolStripMenuItem("Last success: N/A") { Enabled = false };
        lastIpMenuItem = new ToolStripMenuItem("Last IP: N/A") { Enabled = false };
        lastErrorMenuItem = new ToolStripMenuItem("Last error: None") { Enabled = false };
        nextCheckMenuItem = new ToolStripMenuItem("Next check in: N/A") { Enabled = false };

        trayMenu.Items.Add(statusMenuItem);
        trayMenu.Items.Add(lastCheckMenuItem);
        trayMenu.Items.Add(lastSuccessMenuItem);
        trayMenu.Items.Add(lastIpMenuItem);
        trayMenu.Items.Add(lastErrorMenuItem);
        trayMenu.Items.Add(nextCheckMenuItem);
        trayMenu.Items.Add("-");
        trayMenu.Items.Add("DuckDNS Settings", null, OnSettingsClicked);
        trayMenu.Items.Add("Force Update", null, OnForceUpdateClicked);
        trayMenu.Items.Add("Test Config Now", null, OnTestConfigClicked);
        trayMenu.Items.Add("What is my IP Address?", null, OnWhatsMyIpAddressClicked);
        trayMenu.Items.Add("-");
        trayMenu.Items.Add("About", null, OnAboutClicked);
        trayMenu.Items.Add("Exit", null, OnExitClicked);
        RefreshHealthMenuItems();
    }

    private void InitializeTrayIcon()
    {
        Icon trayIconResource = LoadTrayIconSafely();

        trayIcon = new NotifyIcon
        {
            Text = "DuckDns Updater",
            Icon = trayIconResource,
            ContextMenuStrip = trayMenu,
            Visible = true,
        };
        trayIcon.DoubleClick += (s, e) => MessageBox.Show("Right click to choose from one of the menu options!");
    }

    private Icon LoadTrayIconSafely()
    {
        try
        {
            return new Icon("logo.ico", 40, 40);
        }
        catch (Exception ex)
        {
            LogWarning($"Custom tray icon unavailable. Falling back to default icon. {ex.Message}");
            return SystemIcons.Application;
        }
    }

    private void InitializeTimer()
    {
        int refreshInterval = DuckDnsConfigurationRules.NormalizeRefreshInterval(Properties.Settings.Default.RefreshInterval);
        timer = new System.Windows.Forms.Timer { Interval = refreshInterval * 60 * 1000 };
        timer.Tick += OnTimerTick;
        timer.Start();
        nextCheckAt = DateTimeOffset.Now.AddMilliseconds(timer.Interval);

        if (refreshInterval != Properties.Settings.Default.RefreshInterval)
        {
            Properties.Settings.Default.RefreshInterval = refreshInterval;
            Properties.Settings.Default.Save();
        }
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        using SettingsForm form = new();
        form.Owner = this;
        form.StartPosition = FormStartPosition.CenterScreen;

        if (form.ShowDialog() == DialogResult.OK && timer != null)
        {
            int refreshInterval = DuckDnsConfigurationRules.NormalizeRefreshInterval(form.RefreshInterval);
            timer.Interval = refreshInterval * 60 * 1000;
            timer.Start();
            nextCheckAt = DateTimeOffset.Now.AddMilliseconds(timer.Interval);
            RefreshHealthMenuItems();
        }
    }

    private async void OnForceUpdateClicked(object? sender, EventArgs e)
    {
        await UpdateDuckDNSAsync(true);
    }

    private async void OnTestConfigClicked(object? sender, EventArgs e)
    {
        (bool isSuccess, string details) = await RunConfigurationTestAsync();
        MessageBoxIcon icon = isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        MessageBox.Show(details, "DuckDNS Configuration Test", MessageBoxButtons.OK, icon);
    }

    private async void OnWhatsMyIpAddressClicked(object? sender, EventArgs e)
    {
        try
        {
            string ip = await GetPublicIPAsync();
            lastKnownIp = ip;
            SetHealth(HealthState.Ok, "IP query succeeded.");
            MessageBox.Show($"Your External IP Address: {ip}", "IP Address", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            SetHealth(HealthState.Warning, $"IP query failed: {ex.Message}");
            LogError($"Unable to fetch current public IP. {ex.Message}");
            ShowNotification("DuckDNS Updater", "Unable to fetch current public IP.", ToolTipIcon.Warning);
        }
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        string version = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "N/A";
        MessageBox.Show($"DuckDNS Updater (v{version})\nDeveloped by: BlinkSun", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        if (trayIcon != null) trayIcon.Visible = false;
        Application.Exit();
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        await UpdateDuckDNSAsync();
        if (timer != null)
        {
            nextCheckAt = DateTimeOffset.Now.AddMilliseconds(timer.Interval);
        }
    }

    private async Task UpdateDuckDNSAsync(bool forceUpdate = false)
    {
        lastCheckAt = DateTimeOffset.Now;
        try
        {
            string domain = Properties.Settings.Default.Domain;
            string token = TokenProtector.Unprotect(Properties.Settings.Default.Token);

            if (!DuckDnsConfigurationRules.TryValidateConfiguration(domain, token, out string validationError))
            {
                SetHealth(HealthState.Warning, validationError);
                LogWarning($"Invalid configuration. {validationError}");
                HandleFailureSilently(validationError);
                return;
            }

            string newIp = await GetPublicIPAsync();
            lastKnownIp = newIp;
            if (newIp != OldIp || forceUpdate)
            {
                string url = BuildDuckDnsUpdateUrl(domain, token, newIp);
                string result = await GetStringWithRetryAsync(url);

                if (result == "OK")
                {
                    if (Properties.Settings.Default.ShowNotifications)
                    {
                        ShowNotification("DuckDNS Updater", $"IP Address updated to {newIp}", ToolTipIcon.Info);
                    }

                    LogInfo($"DuckDNS update success. IP={newIp}, force={forceUpdate}.");
                    OldIp = newIp;
                    lastSuccessAt = DateTimeOffset.Now;
                    SetHealth(HealthState.Ok, "DuckDNS update succeeded.");
                    SaveRuntimeState();
                    HandleRecoveryIfNeeded();
                }
                else
                {
                    SetHealth(HealthState.Error, $"DuckDNS response was '{result}'.");
                    LogWarning($"DuckDNS update returned unexpected response: {result}.");
                    HandleFailureSilently($"DuckDNS response was '{result}'.");
                }
            }
            else
            {
                lastSuccessAt = DateTimeOffset.Now;
                SetHealth(HealthState.Ok, "IP unchanged, no update needed.");
                LogInfo($"IP unchanged. Skipped update. IP={newIp}.");
                SaveRuntimeState();
                HandleRecoveryIfNeeded();
            }
        }
        catch (Exception ex)
        {
            SetHealth(HealthState.Error, ex.Message);
            LogError($"DuckDNS update failed. {ex.Message}");
            HandleFailureSilently(ex.Message);
        }
        finally
        {
            RefreshHealthMenuItems();
        }
    }

    private void ShowNotification(string title, string message, ToolTipIcon icon)
    {
        if (Properties.Settings.Default.ShowNotifications && trayIcon != null)
        {
            trayIcon.ShowBalloonTip(1000, title, message, icon);
        }
    }

    private async Task<(bool IsSuccess, string Details)> RunConfigurationTestAsync()
    {
        lastCheckAt = DateTimeOffset.Now;

        try
        {
            string domain = Properties.Settings.Default.Domain;
            string token = TokenProtector.Unprotect(Properties.Settings.Default.Token);

            if (!DuckDnsConfigurationRules.TryValidateConfiguration(domain, token, out string validationError))
            {
                SetHealth(HealthState.Warning, validationError);
                RefreshHealthMenuItems();
                return (false, $"Configuration invalid: {validationError}");
            }

            string ip = await GetPublicIPAsync();
            lastKnownIp = ip;
            string url = BuildDuckDnsUpdateUrl(domain, token, ip);
            string duckDnsResult = await GetStringWithRetryAsync(url);

            if (duckDnsResult != "OK")
            {
                SetHealth(HealthState.Error, $"DuckDNS returned '{duckDnsResult}'.");
                RefreshHealthMenuItems();
                return (false, $"DuckDNS returned '{duckDnsResult}'. Check domain/token and try again.");
            }

            OldIp = ip;
            lastSuccessAt = DateTimeOffset.Now;
            SetHealth(HealthState.Ok, "Configuration test succeeded.");
            SaveRuntimeState();
            HandleRecoveryIfNeeded();
            RefreshHealthMenuItems();
            LogInfo($"Configuration test success. IP={ip}.");
            return (true, $"Configuration test passed.\nCurrent public IP: {ip}");
        }
        catch (Exception ex)
        {
            SetHealth(HealthState.Error, ex.Message);
            HandleFailureSilently(ex.Message);
            RefreshHealthMenuItems();
            LogError($"Configuration test failed. {ex.Message}");
            return (false, $"Configuration test failed: {ex.Message}");
        }
    }

    private void OnTrayMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        RefreshHealthMenuItems();
    }

    private void SetHealth(HealthState state, string errorDetails)
    {
        healthState = state;
        lastError = state == HealthState.Ok ? "None" : Shorten(errorDetails, 90);
    }

    private void RefreshHealthMenuItems()
    {
        if (statusMenuItem == null ||
            lastCheckMenuItem == null ||
            lastSuccessMenuItem == null ||
            lastIpMenuItem == null ||
            lastErrorMenuItem == null ||
            nextCheckMenuItem == null)
        {
            return;
        }

        statusMenuItem.Text = $"Status: {healthState}";
        lastCheckMenuItem.Text = $"Last check: {FormatDateTime(lastCheckAt)}";
        lastSuccessMenuItem.Text = $"Last success: {FormatDateTime(lastSuccessAt)}";
        lastIpMenuItem.Text = $"Last IP: {lastKnownIp}";
        lastErrorMenuItem.Text = $"Last error: {lastError}";
        nextCheckMenuItem.Text = $"Next check in: {FormatRemainingTime()}";
    }

    private string FormatRemainingTime()
    {
        if (timer == null || nextCheckAt == default)
        {
            return "N/A";
        }

        TimeSpan remaining = nextCheckAt - DateTimeOffset.Now;
        if (remaining <= TimeSpan.Zero)
        {
            return "Now";
        }

        return $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s";
    }

    private static string FormatDateTime(DateTimeOffset? value)
    {
        return value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
    }

    private static string Shorten(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..(maxLength - 3)]}...";
    }

    private void HandleFailureSilently(string details)
    {
        string signature = Shorten(details, 120);
        hasPendingRecoveryNotification = true;

        if (signature == lastSilentFailureSignature)
        {
            return;
        }

        lastSilentFailureSignature = signature;
        ShowNotification("DuckDNS Updater", "Issue detected. Running in silent retry mode.", ToolTipIcon.Warning);
    }

    private void HandleRecoveryIfNeeded()
    {
        if (!hasPendingRecoveryNotification)
        {
            return;
        }

        hasPendingRecoveryNotification = false;
        lastSilentFailureSignature = string.Empty;
        ShowNotification("DuckDNS Updater", "Service recovered. DuckDNS checks are healthy again.", ToolTipIcon.Info);
    }

    private void LoadRuntimeState()
    {
        try
        {
            if (!File.Exists(StatePath))
            {
                return;
            }

            string[] parts = File.ReadAllText(StatePath).Split('|', StringSplitOptions.None);
            if (parts.Length >= 1 && PublicIpAddressRules.IsRoutablePublicIp(parts[0]))
            {
                OldIp = parts[0];
                lastKnownIp = parts[0];
            }

            if (parts.Length >= 2 && DateTimeOffset.TryParse(parts[1], out DateTimeOffset parsedLastSuccess))
            {
                lastSuccessAt = parsedLastSuccess;
            }
        }
        catch
        {
            // State hydration should never block app startup.
        }
    }

    private void SaveRuntimeState()
    {
        try
        {
            string? dir = Path.GetDirectoryName(StatePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string serialized = $"{OldIp}|{(lastSuccessAt?.ToString("o") ?? string.Empty)}";
            File.WriteAllText(StatePath, serialized);
        }
        catch
        {
            // State persistence should not block updater flow.
        }
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new()
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DuckDnsUpdater/1.1");
        return client;
    }

    private static string BuildDuckDnsUpdateUrl(string domain, string token, string ip)
    {
        string encodedDomain = Uri.EscapeDataString(domain);
        string encodedToken = Uri.EscapeDataString(token);
        string encodedIp = Uri.EscapeDataString(ip);
        return $"https://www.duckdns.org/update?domains={encodedDomain}&token={encodedToken}&ip={encodedIp}";
    }

    private static async Task<string> GetPublicIPAsync()
    {
        List<string> failures = [];

        foreach (string provider in PublicIpProviders)
        {
            try
            {
                string ip = await GetStringWithRetryAsync(provider);
                if (!string.IsNullOrWhiteSpace(ip) && PublicIpAddressRules.IsRoutablePublicIp(ip))
                {
                    return ip;
                }

                failures.Add($"{provider}: returned non-routable or invalid IP '{ip}'.");
            }
            catch (Exception ex)
            {
                failures.Add($"{provider}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"All public IP providers failed. {string.Join(" | ", failures)}");
    }

    private static async Task<string> GetStringWithRetryAsync(string url)
    {
        Exception? lastError = null;

        for (int attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            try
            {
                using CancellationTokenSource timeoutCts = new(RequestTimeout);
                HttpResponseMessage response = await SharedHttpClient.GetAsync(url, timeoutCts.Token);

                if ((int)response.StatusCode >= 500 && attempt < RetryDelays.Length)
                {
                    await Task.Delay(RetryDelays[attempt], CancellationToken.None);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                return content.Trim();
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < RetryDelays.Length)
            {
                lastError = ex;
                await Task.Delay(RetryDelays[attempt], CancellationToken.None);
            }
            catch (Exception ex)
            {
                lastError = ex;
                break;
            }
        }

        throw new InvalidOperationException($"Network call failed for URL: {url}", lastError);
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException || ex is TaskCanceledException || ex is TimeoutException;
    }

    private static void LogInfo(string message) => WriteLog("INFO", message);
    private static void LogWarning(string message) => WriteLog("WARN", message);
    private static void LogError(string message) => WriteLog("ERROR", message);

    private static void WriteLog(string level, string message)
    {
        try
        {
            string? dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string line = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, line);
        }
        catch
        {
            // Logging must never break updater behavior.
        }
    }
}
