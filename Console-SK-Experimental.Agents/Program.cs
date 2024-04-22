
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System.Configuration;
using System.Net;
using Azure.Core;
using System;
using Microsoft.SemanticKernel.Experimental.Agents;
using System.Threading;
using System.ComponentModel;
using Azure;
using Console_SK_Assistant.Plugins;
using Console_SK_Experimental.Agents.Utils;
using System.Security.Cryptography.X509Certificates;


// In this example I demonstrate streaming of the Chat Completion Result
#pragma warning disable SKEXP0101
// Track agents for clean-up
List<IAgent> _agents = new();  // 

Console.WriteLine("This example is us Experimental features and there will be major changes coming!");
Console.WriteLine("In this example we will play around with AI Assistance Agents Features that will soon be coming to >=1.4");

var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");
#pragma warning disable SKEXP0101


var agentInstructions = $@"
You are an Assistant that can detect the intent of the question. The user can ask questions about baking, documents or weather. Your response will be [baking], [documents], [weather] or [unknown] when you cannot predict the intent.

### Baking Example question ###
 How do I bake a cake? 
### End ###

Assistant: [baking]

### Documents Example question ###
 What are our healthcare benefits?
###

Assistant: [documents]

### Weather Example question ###
 What is the weather in Belmont, NC today?
###

Assistant: [weather]

### Unknown Example question ###
 Why do frogs hop?
###

Assistant: [unknown]

You should only response with the intent which is one of the following:
[baking], [documents], [weather] or [unknown]

Do not response with any other details.";

const string uberAgentInstruction = "Only use the available tools to first evaluate the user's intent and invoke the proper tool and provide the exact result.";

KernelPlugin plugin = KernelPluginFactory.CreateFromType<LightOnPlugin>();
KernelPlugin weatherPlugin = KernelPluginFactory.CreateFromType<WeatherPlugin>();

var function = KernelFunctionFactory.CreateFromPrompt(
              agentInstructions,
              functionName: "intentChecker",
              description: "Determine the intent of the objective");

var plugin2 = KernelPluginFactory.CreateFromFunctions("internChecked", "determine intent", new[] { function });

var agentBuilder = new AgentBuilder()
           .WithAzureOpenAIChatCompletion(endpoint: openAiUri!, model: openAiDeployment!, apiKey: openAiApiKey!)
           .WithInstructions(uberAgentInstruction)
           .WithPlugin(plugin)
           .WithPlugin(weatherPlugin)
           .WithPlugin(plugin2);

IAgent intentAgent = await agentBuilder.BuildAsync();

_agents.Add(intentAgent); 

IAgentThread mythread = await intentAgent.NewThreadAsync();

Console.WriteLine("I can help you with baking, weather and questions about Contoso documents. So please keep your questions focused in these areas.");
while (true)
{
    
    Console.Write(">> ");
    var userMessage = Console.ReadLine();
    if (userMessage != "Exit")
    {
        await mythread.AddUserMessageAsync(userMessage!);
        var chatMessage3 = mythread.InvokeAsync(intentAgent);

        await foreach (var message in chatMessage3)
        {
            // Console.WriteLine($"[{message.Id}]");
            Console.WriteLine($"# {message.Role}: {message.Content}");
        }
    }
    else break;
}

// Clean up the Agents

foreach (IAgent agent in _agents)
{
    await agent.DeleteAsync();
}


