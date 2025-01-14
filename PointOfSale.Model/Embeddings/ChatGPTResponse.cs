using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Embeddings
{
    public class ChatGPTResponse
    {
        public int ChatResponseId { get; set; }
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }
        public int TotalTokens { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public string Question {  get; set; }
    }
}
