using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PointOfSale.Business.Embeddings.Models
{
    public class OpenAIEmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<OpenAIEmbeddingData> Data { get; set; }

        [JsonPropertyName("usage")]
        public OpenAIEmbeddingUsage Usage { get; set; }
    }

    public class OpenAIEmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }
    }

    public class OpenAIEmbeddingUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
