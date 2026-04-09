using DuckDnsUpdater.Core;

namespace DuckDnsUpdater;

public class SettingsForm : Form
{
    public int RefreshInterval { get; private set; }

    private Label? lblDomain;
    private Label? lblToken;
    private Label? lblInterval;
    private Label? lblShowNotifications;
    private Label? lblStartWithWindows;
    private TextBox? txtDomain;
    private TextBox? txtToken;
    private ComboBox? cmbInterval;
    private ComboBox? cmbShowNotifications;
    private ComboBox? cmbStartWithWindows;
    private Button? btnSave;
    private Button? btnCancel;

    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        lblDomain = new Label();
        lblToken = new Label();
        lblInterval = new Label();
        lblShowNotifications = new Label();
        lblStartWithWindows = new Label();
        txtDomain = new TextBox();
        txtToken = new TextBox();
        cmbInterval = new ComboBox();
        cmbShowNotifications = new ComboBox();
        cmbStartWithWindows = new ComboBox();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        lblDomain.AutoSize = true;
        lblDomain.Location = new Point(16, 18);
        lblDomain.Name = "lblDomain";
        lblDomain.Size = new Size(56, 15);
        lblDomain.Text = "Domain(s)";

        lblToken.AutoSize = true;
        lblToken.Location = new Point(16, 51);
        lblToken.Name = "lblToken";
        lblToken.Size = new Size(40, 15);
        lblToken.Text = "Token";

        lblInterval.AutoSize = true;
        lblInterval.Location = new Point(16, 84);
        lblInterval.Name = "lblInterval";
        lblInterval.Size = new Size(92, 15);
        lblInterval.Text = "Refresh (minutes)";

        lblShowNotifications.AutoSize = true;
        lblShowNotifications.Location = new Point(16, 117);
        lblShowNotifications.Name = "lblShowNotifications";
        lblShowNotifications.Size = new Size(102, 15);
        lblShowNotifications.Text = "Show notifications";

        lblStartWithWindows.AutoSize = true;
        lblStartWithWindows.Location = new Point(16, 150);
        lblStartWithWindows.Name = "lblStartWithWindows";
        lblStartWithWindows.Size = new Size(114, 15);
        lblStartWithWindows.Text = "Start with Windows";

        txtDomain.Location = new Point(152, 15);
        txtDomain.Name = "txtDomain";
        txtDomain.Size = new Size(210, 23);
        txtDomain.TabIndex = 0;

        txtToken.Location = new Point(152, 48);
        txtToken.Name = "txtToken";
        txtToken.Size = new Size(210, 23);
        txtToken.TabIndex = 1;
        txtToken.UseSystemPasswordChar = true;

        cmbInterval.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbInterval.FormattingEnabled = true;
        cmbInterval.Items.AddRange(["5", "10", "15", "30", "60"]);
        cmbInterval.Location = new Point(152, 81);
        cmbInterval.Name = "cmbInterval";
        cmbInterval.Size = new Size(210, 23);
        cmbInterval.TabIndex = 2;

        cmbShowNotifications.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbShowNotifications.FormattingEnabled = true;
        cmbShowNotifications.Items.AddRange(["YES", "NO"]);
        cmbShowNotifications.Location = new Point(152, 114);
        cmbShowNotifications.Name = "cmbShowNotifications";
        cmbShowNotifications.Size = new Size(210, 23);
        cmbShowNotifications.TabIndex = 3;

        cmbStartWithWindows.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStartWithWindows.FormattingEnabled = true;
        cmbStartWithWindows.Items.AddRange(["YES", "NO"]);
        cmbStartWithWindows.Location = new Point(152, 147);
        cmbStartWithWindows.Name = "cmbStartWithWindows";
        cmbStartWithWindows.Size = new Size(210, 23);
        cmbStartWithWindows.TabIndex = 4;

        btnSave.Location = new Point(206, 188);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(75, 28);
        btnSave.TabIndex = 5;
        btnSave.Text = "Save";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Click += BtnSave_Click;

        btnCancel.Location = new Point(287, 188);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(75, 28);
        btnCancel.TabIndex = 6;
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += BtnCancel_Click;

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(380, 231);
        Controls.Add(btnCancel);
        Controls.Add(btnSave);
        Controls.Add(cmbStartWithWindows);
        Controls.Add(cmbShowNotifications);
        Controls.Add(cmbInterval);
        Controls.Add(txtToken);
        Controls.Add(txtDomain);
        Controls.Add(lblStartWithWindows);
        Controls.Add(lblShowNotifications);
        Controls.Add(lblInterval);
        Controls.Add(lblToken);
        Controls.Add(lblDomain);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "DuckDNS Settings";
        ResumeLayout(false);
        PerformLayout();
    }

    private void LoadSettings()
    {
        string domain = DuckDnsConfigurationRules.NormalizeDomain(Properties.Settings.Default.Domain);
        string plainToken = DuckDnsConfigurationRules.NormalizeToken(TokenProtector.Unprotect(Properties.Settings.Default.Token));
        int refreshInterval = DuckDnsConfigurationRules.NormalizeRefreshInterval(Properties.Settings.Default.RefreshInterval);
        bool startWithWindows = StartupManager.IsEnabled();

        if (txtDomain != null) txtDomain.Text = domain;
        if (txtToken != null) txtToken.Text = plainToken;
        if (cmbInterval != null) cmbInterval.SelectedItem = refreshInterval.ToString();
        if (cmbShowNotifications != null) cmbShowNotifications.SelectedItem = Properties.Settings.Default.ShowNotifications ? "YES" : "NO";
        if (cmbStartWithWindows != null) cmbStartWithWindows.SelectedItem = startWithWindows ? "YES" : "NO";

        string protectedToken = TokenProtector.Protect(plainToken);
        if (domain != Properties.Settings.Default.Domain ||
            protectedToken != Properties.Settings.Default.Token ||
            refreshInterval != Properties.Settings.Default.RefreshInterval)
        {
            Properties.Settings.Default.Domain = domain;
            Properties.Settings.Default.Token = protectedToken;
            Properties.Settings.Default.RefreshInterval = refreshInterval;
            Properties.Settings.Default.Save();
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        string domain = DuckDnsConfigurationRules.NormalizeDomain(txtDomain?.Text);
        string token = DuckDnsConfigurationRules.NormalizeToken(txtToken?.Text);

        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(token))
        {
            MessageBox.Show("Domain and Token are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!DuckDnsConfigurationRules.IsValidDuckDnsDomain(domain))
        {
            MessageBox.Show(
                "Invalid domain. Use one or more DuckDNS subdomains (letters/numbers/hyphen), separated by commas.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!Guid.TryParse(token, out _))
        {
            MessageBox.Show(
                "Invalid token format. A DuckDNS token should look like a GUID (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        int refreshInterval = DuckDnsConfigurationRules.NormalizeRefreshInterval(int.Parse(cmbInterval?.SelectedItem?.ToString() ?? DuckDnsConfigurationRules.MinRefreshIntervalMinutes.ToString()));
        bool shouldStartWithWindows = cmbStartWithWindows?.SelectedItem?.ToString() == "YES";

        try
        {
            StartupManager.SetEnabled(shouldStartWithWindows);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to update startup behavior: {ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Properties.Settings.Default.Domain = domain;
        Properties.Settings.Default.Token = TokenProtector.Protect(token);
        Properties.Settings.Default.RefreshInterval = refreshInterval;
        Properties.Settings.Default.ShowNotifications = cmbShowNotifications?.SelectedItem?.ToString() == "YES";
        Properties.Settings.Default.Save();

        RefreshInterval = refreshInterval;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
