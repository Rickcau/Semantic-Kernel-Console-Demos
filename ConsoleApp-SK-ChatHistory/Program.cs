using ConsoleApp_SK_ChatHistory.Service;
using Cosmos.Chat.GPT.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Configuration;
using System.Text.Json;
using System.Security.Principal;


Console.WriteLine("NOT Finished with this yet! - Semantic Kernel ChatHistory Store & RetreiveBot\n");

// 5 Simple steps to use the Kernel!

//#region Step 1: Create Kernel Builder
//// Create a Builder for Creating Kernel Objects
//var builder = Kernel.CreateBuilder();
//#endregion

//#region Step 2: Load AI Endpoint Values

//var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
//var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
//var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");

//#endregion

//#region Step 3: Add ChatCompletion Service

//builder.Services.AddAzureOpenAIChatCompletion(
//   deploymentName: openAiDeployment!,
//   endpoint: openAiUri!,
//   apiKey: openAiApiKey!);

//#endregion

//#region Step 4: Construct Kernel, ChatHistory Get instance of ChatCompletion Service
//// Construct instance of Kernel using Builder Settings
//var kernel = builder.Build();

//ChatHistory history = [];

//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//#endregion

//#region Step 5: Send Prompt Get Response - Exercise One

//// var prompt = "Why is the Sky blue?";
////var result = await chatCompletionService.GetChatMessageContentAsync(prompt);
////Console.WriteLine(result);
////Console.WriteLine("\nPress enter to end.");
////Console.ReadLine();

//#endregion

//#region Chat Loop - Exercise Two

//while (true)
//{
//    Console.Write(">> ");
//    var userMessage = Console.ReadLine();
//    if (userMessage != "Exit")
//    {
//        history.AddUserMessage(userMessage!);

//        // Not really being used in this example but we will use it in future examples
//        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
//        {
//            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
//        };

//        try
//        {
//            var result = await chatCompletionService.GetChatMessageContentAsync(
//                history,
//                executionSettings: openAIPromptExecutionSettings,
//                kernel: kernel);

//            Console.WriteLine("<< " + result);

//            if (result.Content != null)
//            {
//                history.AddAssistantMessage(result.Content);
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error: {ex.Message}");
//        }
//    }
//    else break;
//}

//#endregion

var endpoint = ConfigurationManager.AppSettings.Get("CosmosDbConnectionString") ?? "";
var databaseId = ConfigurationManager.AppSettings.Get("CosmosDbId") ?? "";
var containerId = ConfigurationManager.AppSettings.Get("CosmosContainerChatHistoryId") ?? "";

CosmosDBService cosmosService = new CosmosDBService(endpoint, databaseId, containerId);

// Create a new ChatUser  
ChatUser newUser = new ChatUser
{
    UserName = "testuser"
};
var userId = await cosmosService.InsertChatUserAsync(newUser);
Console.WriteLine($"Created ChatUser with ID: {userId}");

// Retrieve the ChatUser by username  
string retrievedUserId = await cosmosService.GetChatUserIdByUsernameAsync("testuser");
Console.WriteLine($"Retrieved ChatUser ID: {retrievedUserId}");

// Create a new Session for the user  
Session newSession = new Session
{
    ChatUserId = userId,
    Name = "THis is a test",
    StartTime = DateTime.UtcNow
};
string sessionId = await cosmosService.CreateSessionAsync(newSession);
Console.WriteLine($"Created Session with ID: {sessionId}");

// Create a new Message in the session  
Message newMessage = new Message
{
    SessionId = sessionId,
    ChatUserId = userId,
    Prompt = "Hello, how are you?",
    Completion = "I'm good, thank you!",
    PromptTokens = 5,
    CompletionTokens = 4,
    Timestamp = DateTime.UtcNow
};
string messageId = await cosmosService.CreateMessageAsync(newMessage);
Console.WriteLine($"Created Message with ID: {messageId}");

// Create a new Message in the session  
Message newMessage2 = new Message
{
    SessionId = sessionId,
    ChatUserId = userId,
    Prompt = "Hello, how are you 2?",
    Completion = "I'm good, thank you 2!",
    PromptTokens = 5,
    CompletionTokens = 4,
    Timestamp = DateTime.UtcNow
};

string messageId2 = await cosmosService.CreateMessageAsync(newMessage);
Console.WriteLine($"Created Message with ID: {messageId2}");

// Retrieve the ChatUser details  
ChatUser retrievedUser = await cosmosService.GetChatUserAsync(userId);
Console.WriteLine($"Retrieved ChatUser: {retrievedUser.UserName}");

// Retrieve all sessions for the user  

var sessions = await cosmosService.GetSessionsByChatUserIdAsync(userId);
Console.WriteLine($"Sessions: {sessions.Count}");
Console.WriteLine($"Session : {sessions[0].SessionId}");
Console.WriteLine($"Session : {sessions[0].Name}");
// Lets serialize into a JSON object
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
};
var sessionsJson = JsonSerializer.Serialize<List<Session>>(sessions, options);

Console.WriteLine($"Sessions: {sessionsJson}");

// Now lets get the Messages for a specific SessionID

var messages = await cosmosService.GetMessagesBySessionIdAsync(sessionId);
Console.WriteLine($"Message Count: {messages.Count}");
Console.WriteLine($"Messages: {messages}");

var messagesJson = JsonSerializer.Serialize<List<Message>>(messages, options);
Console.WriteLine($"MessagesJson: {messagesJson}");
Console.ReadLine();

