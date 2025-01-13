using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using PointOfSale.Model.Embeddings;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;

namespace PointOfSale.Business.Embeddings
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly HttpClient _httpClient;
        private readonly string _model = "sentence-transformers/all-MiniLM-L6-v2";
        private readonly string hfToken = "ver en blog";
        private readonly string _url = "https://api-inference.huggingface.co/models/";

        public EmbeddingService(IUnitOfWork unitOfWork)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", hfToken);
            _unitOfWork = unitOfWork;
        }

        public async Task<float[]> GenerateEmbedding(string text)
        {
            var payload = new { inputs = text };
            var jsonPayload = JsonSerializer.Serialize(payload);

            var response = await _httpClient.PostAsync(
                $"{_url}{_model}",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            string responseJson = await response.Content.ReadAsStringAsync();
            float[][]? embeddings = JsonSerializer.Deserialize<float[][]>(responseJson);
            if (embeddings == null || embeddings.Length == 0)
                throw new Exception("No se pudo obtener el embedding");

            return embeddings[0];
        }

        public Embedding CreateEmbedding(string Reference, string source, float[] embeddingVector)
        {
            return new Embedding
            {
                Reference = Reference,
                Source = source,
                Vector = ConvertVectorToBinary(embeddingVector),
                CreatedAt = TimeHelper.GetArgentinaTime()
            };

        }

        public async Task SaveRangeEmbeddingAsync(List<Embedding> embeddings)
        {
            var embeddingRepository = _unitOfWork.Repository<Embedding>();
            await embeddingRepository.AddRange(embeddings);
        }

        private byte[] ConvertVectorToBinary(float[] vector)
        {
            var byteArray = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        private float[] ConvertBinaryToVector(byte[] byteArray)
        {
            var vector = new float[byteArray.Length / sizeof(float)];
            Buffer.BlockCopy(byteArray, 0, vector, 0, byteArray.Length);
            return vector;
        }
    }
}
