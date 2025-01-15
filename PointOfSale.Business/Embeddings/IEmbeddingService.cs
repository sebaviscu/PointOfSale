using PointOfSale.Business.Embeddings.Models;
using PointOfSale.Model.Embeddings;

namespace PointOfSale.Business.Embeddings
{
    public interface IEmbeddingService
    {
        Task<List<ChatGPTResponse>> GetChatByIdUser(int idUser, int idTienda);

        Task<OpenAIEmbeddingResponse> GenerateEmbedding(string text);

        Embedding CreateEmbedding(string Reference, string source, float[] embeddingVector, int promptTokens, int idTienda);

        Task SaveRangeEmbeddingAsync(List<Embedding> embeddings);

        Task<ChatGPTResponse> GetChatResponseAsync(string userQuestion,int idUser, int idTienda);

        Task<int> GetTokensByTienda(int idTienda, DateTime dateFrom);
    }
}
