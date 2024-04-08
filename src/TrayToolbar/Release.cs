#nullable disable

using System.Text.Json.Serialization;

namespace TrayToolbar
{
    internal class Release
    {
        [JsonPropertyName("update_url")]
        public string UpdateUrl { get; set; }

        [JsonPropertyName("tag_name")]
        public string Name { get; set; }
    }
}