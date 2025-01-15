using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using PointOfSale.Business.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PointOfSale.Business.SemanticKernel
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly string _apiKey = "";
        private readonly string _modelChat = "";
        private Kernel kernel;

        public SemanticKernelService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _modelChat = configuration["OpenAI:Model"];

            Configuration();
        }

        public void Configuration()
        {
            // Create a kernel with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(_modelChat, _apiKey);

            // Build the kernel
            kernel = builder.Build();


            // Add a plugin (the LightsPlugin class is defined below)
            kernel.Plugins.AddFromType<SalesPlugin>("Sales");
        }

        public async Task<string> Chat(string question)
        {
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Create a history store the conversation
            var history = new ChatHistory();
            history.AddUserMessage(question);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
               history,
               executionSettings: openAIPromptExecutionSettings,
               kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddAssistantMessage(result.Content);

            return result.Content;
        }
    }
}
