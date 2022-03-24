using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Moosta.Shared.Platform.Models
{
    public class MoostaIdea
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonProperty("createddate")]
        [JsonPropertyName("createddate")]
        public long CreatedDate { get; set; }

        [JsonProperty("createdby")]
        [JsonPropertyName("createdby")]
        public string? CreatedByUserId { get; set; }

        [JsonProperty("ispublic")]
        [JsonPropertyName("ispublic")]
        public bool IsPublic { get; set; }
    }
}
