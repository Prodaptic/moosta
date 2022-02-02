using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Moosta.Shared.Platform.Models
{
    public class MoostaCompletion
    {
        [JsonPropertyName("completiontext")]
        public string CompletionText { get; set; }
    }
}
