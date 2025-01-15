using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PointOfSale.SemanticKernel.Plugins;

public class Program
{
    public static async Task Main(string[] args)
    {
        var modelId = "gpt-3.5-turbo";
        var apiKey = "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";

        // Create a kernel with Azure OpenAI chat completion
        var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, apiKey);

        // Build the kernel
        Kernel kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Add a plugin (the LightsPlugin class is defined below)
        kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        kernel.Plugins.AddFromType<SalesPlugin>("Sales");

        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Create a history store the conversation
        var history = new ChatHistory();
        history.AddUserMessage("Please turn on the lamp");

        // Get the response from the AI
        var result = await chatCompletionService.GetChatMessageContentAsync(
           history,
           executionSettings: openAIPromptExecutionSettings,
           kernel: kernel);

        // Print the results
        Console.WriteLine("Assistant > " + result);

        // Add the message from the agent to the chat history
        history.AddAssistantMessage(result.Content);
        //history.AddAssistantMessage(result);
    }
}
