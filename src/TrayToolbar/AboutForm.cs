using TrayToolbar.Extensions;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar;

public partial class AboutForm : Form
{
    static readonly (string Name, string Url)[] SponsorLinks = [
        ("SignPath.io", AboutInfo.SignPathUrl),
        ("SignPath Foundation", AboutInfo.SignPathFoundationUrl),
    ];

    internal event EventHandler<UpdateCheckResult>? UpdateCheckCompleted;

    private UpdateCheckResult? _lastResult;
    private DateTime? _lastCheckedUtc;
    private bool _justUpdated;

    public AboutForm() : this(null, null, false)
    {
    }

    internal AboutForm(UpdateCheckResult? lastResult, DateTime? lastCheckedUtc, bool justUpdated)
    {
        InitializeComponent();
        _lastResult = lastResult;
        _lastCheckedUtc = lastCheckedUtc;
        _justUpdated = justUpdated;
        LoadResources();
        ShowStatus();
        HandleCreated += SetTheme;
        if (Handle != 0)
        {
            SetTheme(null, EventArgs.Empty);
        }
    }

    private void LoadResources()
    {
        Text = R.About;
        TitleLabel.Text = R.TrayToolbar;
        VersionLabel.Text = string.Format(R.Version, AboutInfo.Version, AboutInfo.Architecture);
        RuntimeLabel.Text = AboutInfo.Runtime;
        CopyrightLabel.Text = AboutInfo.Copyright;
        GitHubLink.Text = R.TrayToolbar_on_GitHub;
        IssuesLink.Text = R.Report_an_issue;
        SponsorLink.Text = R.SignPath_sponsor;
        SponsorLink.Links.Clear();
        foreach (var (name, url) in SponsorLinks)
        {
            var start = SponsorLink.Text.IndexOf(name, StringComparison.Ordinal);
            if (start >= 0)
            {
                SponsorLink.Links.Add(start, name.Length, url);
            }
        }
        CheckUpdatesButton.Text = R.Check_for_updates;
        UpdateNowButton.Text = R.Update_now;
        OkButton.Text = R.OK;
    }

    private void ShowStatus()
    {
        StatusLabel.Text = _lastResult?.Status switch
        {
            UpdateCheckStatus.UpdateAvailable => $"{R.A_new_version_is_available} ({_lastResult.Version})",
            UpdateCheckStatus.Prerelease => R.You_are_using_a_prerelease_version,
            UpdateCheckStatus.UpToDate => R.You_are_up_to_date,
            UpdateCheckStatus.Failed => R.Could_not_check_for_updates,
            _ => _justUpdated ? string.Format(R.Updated_to_version, AboutInfo.Version) : string.Empty,
        };
        UpdateNowButton.Enabled = _lastResult?.Status == UpdateCheckStatus.UpdateAvailable;
        LastCheckedLabel.Visible = _lastCheckedUtc.HasValue;
        if (_lastCheckedUtc.HasValue)
        {
            LastCheckedLabel.Text = string.Format(R.Last_checked, _lastCheckedUtc.Value.ToLocalTime().ToString("g"));
        }
    }

    private void SetTheme(object? sender, EventArgs e)
    {
        var darkmode = SystemTheme.DarkModeEnabled == true;
        SystemTheme.UseImmersiveDarkMode(Handle, darkmode);
        SystemTheme.SetThemeColors(this, darkmode);
    }

    private async void CheckUpdatesButton_Click(object sender, EventArgs e)
    {
        CheckUpdatesButton.Enabled = false;
        UpdateNowButton.Enabled = false;
        StatusLabel.Text = R.Checking_for_updates;
        var result = await UpdateChecker.CheckAsync();
        if (IsDisposed)
        {
            return;
        }
        _lastResult = result;
        _lastCheckedUtc = DateTime.UtcNow;
        _justUpdated = false;
        ShowStatus();
        CheckUpdatesButton.Enabled = true;
        UpdateCheckCompleted?.Invoke(this, result);
    }

    private void UpdateNowButton_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(this,
            R.Are_you_sure_you_want_to_update_to_the_latest_version,
            R.Update_TrayToolbar,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            ConfigHelper.UpdateToLatestVersion();
        }
    }

    private void GitHubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Program.Launch(AboutInfo.ReleasesUrl);
    }

    private void IssuesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Program.Launch(AboutInfo.IssuesUrl);
    }

    private void SponsorLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (e.Link?.LinkData is string url)
        {
            Program.Launch(url);
        }
    }
}