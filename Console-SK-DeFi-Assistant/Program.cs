﻿
using Console_SK_DeFi_Assistant.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System.Configuration;
using System.Net;

Console.WriteLine("This example makes use of an SK Plugin that calls an endpoint and use Chat History in a very simple Assistant Scenario");
Console.WriteLine("Since this example is using Plugin that Queries the Uniswap V3 Subgraph you need to ask questions that make sense to be used in a query.");
Console.WriteLine("Example 1: top 10 active pools");
Console.WriteLine("Example 2: Retrieve 10 most liquid pools");
Console.WriteLine("{ pools(orderBy: volumeUSD, orderDirection: desc, first: 10) { id volumeUSD } }");
Console.WriteLine("Type: Exit to exit the chat");
Console.WriteLine("Streaming is NOT used!");


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
builder.Plugins.AddFromType<UniswapV3SubgraphPlugin>();

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

        try
        {
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            Console.WriteLine("<< " + result);

            if (result.Content != null)
            {
                history.AddAssistantMessage(result.Content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else break;
}

