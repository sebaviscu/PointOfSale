using PointOfSale.Business.Embeddings.Models;
using PointOfSale.Model.Embeddings;

namespace PointOfSale.Business.Embeddings
{
    public interface IEmbeddingService
    {
        Task<OpenAIEmbeddingResponse> GenerateEmbedding(string text);

        Embedding CreateEmbedding(string Reference, string source, float[] embeddingVector, int promptTokens);

        Task SaveRangeEmbeddingAsync(List<Embedding> embeddings);

        Task<string> GenerateResponseAsync(string userQuestion);
    }
}
