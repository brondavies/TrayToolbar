using System.Net.Http.Json;

using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal sealed class GitHubReleaseClient : IReleaseClient
{
    const string UpdateUrl = "https://github.com/brondavies/TrayToolbar/releases/latest";

    public async Task<Release?> GetLatestReleaseAsync()
    {
        Release? version = null;
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        try
        {
            version = await client.GetFromJsonAsync<Release?>(UpdateUrl);
        }
        catch { }
        return version;
    }
}