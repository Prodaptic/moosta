using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Moosta.Shared.Platform.Models
{
    public class MoostaUser
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonProperty("authid")]
        [JsonPropertyName("authid")]
        public string? AuthId { get; set; }

        [JsonProperty("registereddate")]
        [JsonPropertyName("registereddate")]
        public long RegisteredDate { get; set; }

        [JsonProperty("email")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonProperty("roles")]
        [JsonPropertyName("roles")]
        public string? Roles { get; set; }

    }
}
