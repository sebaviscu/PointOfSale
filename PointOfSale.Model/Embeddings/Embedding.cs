using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PointOfSale.Model.Embeddings
{
    public class Embedding
    {

        public int EmbeddingId { get; set; }
        public string Reference { get; set; } // id de lo que guardo
        public string Source { get; set; } // texto original
        public byte[] Vector { get; set; } // Embedding
        public DateTime CreatedAt { get; set; }
        public int PromptTokens { get; set; }
        public double Norm { get; set; }

    }
}
