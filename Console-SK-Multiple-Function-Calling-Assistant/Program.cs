using Console_SK_Assistant.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System.Configuration;
using System.Net;
using Azure.Core;
using System;

// In this example I demonstrate streaming of the Chat Completion Result

Console.WriteLine("This example uses two plugins and AutoInvokeKernelFunctions");
Console.WriteLine("Examples:");
Console.WriteLine("Is the light on?");
Console.WriteLine("Please turn the light on");
Console.WriteLine("2+2");
Console.WriteLine("2x2");
Console.WriteLine("Is the light on?");

var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "JSONPluginYaml");
var getJSONYaml = File.ReadAllText($"{pluginsDirectory}\\getJSON.yaml"); 
var writeBusinessEmail = File.ReadAllText($"{pluginsDirectory}\\WriteBusinessMail.yaml");

var builder = Kernel.CreateBuilder();

var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");

if (openAiDeployment != null && openAiUri != null && openAiApiKey != null)
{
    builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: openAiDeployment,
    endpoint: openAiUri,
    apiKey: openAiApiKey);
}
builder.Plugins.AddFromType<MathPlugin>();
builder.Plugins.AddFromType<LightOnPlugin>();
builder.Plugins.AddFromFunctions(writeBusinessEmail, new KernelFunction[] { KernelFunctionYaml.FromPromptYaml("yaml") });



var kernel = builder.Build();

ChatHistory history = [];

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


while (true)
{
    Console.Write(">> ");
    var userMessage = Console.ReadLine();
    if (userMessage != "Exit")
    {
        // history.AddUserMessage(Console.ReadLine()!);
        history.AddUserMessage(userMessage!);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        bool roleWritten = false;
        string fullMessage = string.Empty;

        try
        {
            await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: kernel))
            {
                if (!roleWritten && chatUpdate.Role.HasValue)
                {
                    Console.Write($"{chatUpdate.Role.Value}: {chatUpdate.Content}");
                    roleWritten = true;
                }

                if (chatUpdate.Content is { Length: > 0 })
                {
                    fullMessage += chatUpdate.Content;
                    Console.Write(chatUpdate.Content);
                }
            }

            Console.WriteLine("\n------------------------");
            history.AddMessage(AuthorRole.Assistant, fullMessage);
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else break;
}



