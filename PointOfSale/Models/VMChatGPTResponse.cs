using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMChatGPTResponse
    {
        public int ChatResponseId { get; set; }
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }
        public int TotalTokens { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public string Question { get; set; }
        public int IdUser { get; set; }
        public int IdTienda { get; set; }
    }
}
