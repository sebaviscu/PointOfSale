using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Model.Embeddings;

namespace PointOfSale.Business.Embeddings
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbedding(string text);

        Embedding CreateEmbedding(string Reference, string source, float[] embeddingVector);

        Task SaveRangeEmbeddingAsync(List<Embedding> embeddings);
    }
}
