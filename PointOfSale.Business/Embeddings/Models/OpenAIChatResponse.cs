using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PointOfSale.Business.Embeddings.Models
{
    public class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }

    public class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; }
    }

    public class ChatMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public string PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public string CompletionTokens { get; set; }
    }
}
