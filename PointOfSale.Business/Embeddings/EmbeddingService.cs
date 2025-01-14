using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using PointOfSale.Model.Embeddings;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Embeddings.Models;

namespace PointOfSale.Business.Embeddings
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly string _modelEmbeddings;
        private readonly string _modelChat;
        private readonly string _apiUrl;
        private readonly string _apiKey;

        public EmbeddingService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _modelEmbeddings = configuration["OpenAI:EmbeddingModel"];
            _apiUrl = configuration["OpenAI:ApiUrl"];
            _modelChat = configuration["OpenAI:Model"];

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", _apiKey);
            _unitOfWork = unitOfWork;
        }

        public async Task<OpenAIEmbeddingResponse> GenerateEmbedding(string text)
        {
            var payload = new
            {
                input = text,
                model = _modelEmbeddings
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var response = await _httpClient.PostAsync(
                _apiUrl + "/embeddings",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            string responseJson = await response.Content.ReadAsStringAsync();

            var jsonResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);
            if (jsonResponse == null || jsonResponse.Data == null || jsonResponse.Data.Count == 0)
                throw new Exception("No se pudo obtener el embedding");

            return jsonResponse;
        }

        public async Task<string> GenerateResponseAsync(string userQuestion)
        {
            var embeddingQuestion = await GenerateEmbedding(userQuestion);

            var embeddingsList = await GetMostSimilarEmbeddingsAsync(embeddingQuestion.Data[0].Embedding);

            var context = CreateContext(embeddingsList);

            var prompt = $@"
                        Aquí tienes información relevante:
                        {context}

                        Pregunta: '{userQuestion}'
                        Por favor, genera una respuesta basada en la información proporcionada.";

            var gptResponse = await GenerateGptResponseAsync(prompt, userQuestion);

            return gptResponse;
        }

        public Embedding CreateEmbedding(string Reference, string source, float[] embeddingVector, int promptTokens)
        {
            return new Embedding
            {
                Reference = Reference,
                Source = source,
                Vector = ConvertVectorToBinary(embeddingVector),
                CreatedAt = TimeHelper.GetArgentinaTime(),
                PromptTokens = promptTokens,
                Norm = CalculateNorm(embeddingVector),
            };
        }

        public async Task SaveRangeEmbeddingAsync(List<Embedding> embeddings)
        {
            var embeddingRepository = _unitOfWork.Repository<Embedding>();
            await embeddingRepository.AddRange(embeddings);
        }

        private async Task<string> GenerateGptResponseAsync(string prompt, string question)
        {
            var payload = new
            {
                model = _modelChat,
                messages = new[]
                {
            new { role = "system", content = "Eres un asistente inteligente que responde preguntas basadas en datos de ventas, gastos y productos." },
            new { role = "user", content = prompt }
            },
                max_tokens = 200,
                temperature = 0.7
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var response = await _httpClient.PostAsync(
                _apiUrl + "/chat/completions",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var responseJson = await response.Content.ReadAsStringAsync();

            var chatGPTResponse = ParseChatResponse(responseJson, question);
            await SaveChatResponseAsync(chatGPTResponse);

            var jsonResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(responseJson);

            return jsonResponse.Choices[0].Message.Content;
        }

        private async Task SaveChatResponseAsync(ChatGPTResponse response)
        {
            var chatResponseRepository = _unitOfWork.Repository<ChatGPTResponse>();
            await chatResponseRepository.Add(response);
        }
         
        private ChatGPTResponse ParseChatResponse(string jsonResponse, string question)
        {
            var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            return new ChatGPTResponse
            {
                Id = root.GetProperty("id").GetString(),
                CreatedAt = TimeHelper.GetArgentinaTime(),
                Content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString(),
                TotalTokens = root.GetProperty("usage").GetProperty("total_tokens").GetInt32(),
                PromptTokens = root.GetProperty("usage").GetProperty("prompt_tokens").GetInt32(),
                CompletionTokens = root.GetProperty("usage").GetProperty("completion_tokens").GetInt32(),
                Question = question
            };
        }

        private string CreateContext(List<Embedding> similarEmbeddings)
        {
            var contextBuilder = new StringBuilder("Información relevante encontrada:\n");
            foreach (var embedding in similarEmbeddings)
            {
                contextBuilder.AppendLine($"- {embedding.Reference}: {embedding.Source}");
            }

            return contextBuilder.ToString();
        }

        private async Task<List<Embedding>> GetMostSimilarEmbeddingsAsync(float[] questionEmbedding)
        {
            var questionNorm = CalculateNorm(questionEmbedding);

            var embeddings = await _unitOfWork.Repository<Embedding>().Query();

            var candidates = await embeddings
                    .OrderBy(e => Math.Abs(e.Norm - questionNorm))
                    .Take(10)
                    .ToListAsync();


            var similarityScores = candidates
                .Select(e => new
                {
                    Embedding = e,
                    Similarity = CosineSimilarity(ConvertBinaryToVector(e.Vector), questionEmbedding)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(5)
                .Select(x => x.Embedding)
                .ToList();

            return similarityScores;
        }

        private double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Los vectores deben tener la misma longitud.");

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += Math.Pow(vectorA[i], 2);
                magnitudeB += Math.Pow(vectorB[i], 2);
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            return dotProduct / (magnitudeA * magnitudeB);
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

        private double CalculateNorm(float[] vector)
        {
            double norm = 0;
            foreach (var value in vector)
            {
                norm += value * value;
            }
            return Math.Sqrt(norm);
        }
    }
}
