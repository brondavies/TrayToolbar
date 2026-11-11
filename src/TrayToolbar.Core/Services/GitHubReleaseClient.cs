using System.Net.Http.Headers;
using System.Net.Http.Json;

using TrayToolbar.Models;

namespace TrayToolbar.Services;

internal sealed class GitHubReleaseClient : IReleaseClient
{
    const string LatestReleaseApiUrl = "https://api.github.com/repos/brondavies/TrayToolbar/releases/latest";

    public async Task<Release?> GetLatestReleaseAsync()
    {
        Release? release = null;
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TrayToolbar", ConfigHelper.ApplicationVersion));
        try
        {
            release = await client.GetFromJsonAsync<Release?>(LatestReleaseApiUrl);
        }
        catch { }
        return release;
    }
}