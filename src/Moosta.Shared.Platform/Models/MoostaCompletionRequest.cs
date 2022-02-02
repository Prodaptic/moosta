using System.Text.Json.Serialization;

namespace Moosta.Shared.Platform.Models
{
    public class MoostaCompletionRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }
    }
}
