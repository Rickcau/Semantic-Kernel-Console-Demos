using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Threading.Tasks;
using Cosmos.Chat.GPT.Models;
using System.Text.Json;
using System.Net;

namespace ConsoleApp_SK_ChatHistory.Service;

public class CosmosDBService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public CosmosDBService(string endpoint, string databaseId, string containerId)
    {
        CosmosSerializationOptions options = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase  // Allows use to using camelCase and not need for JsonProperty attribute on our POCO classes
        };

        CosmosClient client = new CosmosClientBuilder(endpoint)
            .WithSerializerOptions(options)
            .Build();

        Database database = client.GetDatabase(databaseId)!;
        Container chatContainer = database.GetContainer(containerId)!;

        _cosmosClient = client;
        _container = chatContainer;
        // _container = _cosmosClient.GetContainer(databaseId, containerId);
    }

    public async Task<String> InsertChatUserAsync(ChatUser chatuser)
    {
        try
        {
            var _id = Guid.NewGuid().ToString();
            chatuser.Id = _id;
            chatuser.ChatUserId = _id;
            chatuser.DocumentType = "chatUser";
            PartitionKey partitionKey = new(_id);
            ChatUser newChatUser = chatuser;
            await _container.CreateItemAsync<ChatUser>(item: chatuser, partitionKey: partitionKey);
            return chatuser.ChatUserId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GetChatUserIdByUsernameAsync(string username)
    {
        var query = new QueryDefinition("SELECT c.chatUserId FROM c WHERE c.userName = @username")
            .WithParameter("@username", username);

        var iterator = _container.GetItemQueryIterator<ChatUser>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var chatuser = response.FirstOrDefault();
            if (chatuser != null && chatuser.ChatUserId != null)
            {
                return chatuser.ChatUserId;
            }
        }
        throw new InvalidOperationException($"User with username '{username}' not found.");
    }

    public async Task<string> CreateSessionAsync(Session session)
    {
        try
        {
            var _id = Guid.NewGuid().ToString();
            session.Id = _id;
            session.SessionId = _id;
            session.DocumentType = "chatSession";
            PartitionKey partitionKey = new(_id);

            // Create the session
            await _container.CreateItemAsync<Session>(item: session, partitionKey: partitionKey);

            // Now, add the session ID to the ChatUser's session array
            if (session.ChatUserId != null)
            {
                await AddSessionToChatUserAsync(session.ChatUserId, session);
            }

            return session.SessionId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }

    private async Task AddSessionToChatUserAsync(string chatUserId, Session session)
    {
        try
        {
            // Retrieve the ChatUser document
            ChatUser chatUser = await GetChatUserAsync(chatUserId);

            // Ensure the sessions array is initialized
            if (chatUser.Sessions != null)
            {
                chatUser.Sessions.Add(session);
            }

            // Update the ChatUser document in Cosmos DB
            PartitionKey partitionKey = new(chatUser.ChatUserId);
            await _container.ReplaceItemAsync(chatUser, chatUser.ChatUserId, partitionKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while updating ChatUser: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CreateMessageAsync(Message message)
    {
        try
        {
            var _id = Guid.NewGuid().ToString();
            message.Id = _id;
            message.MessageId = _id;
            message.DocumentType = "chatMessage";
            PartitionKey partitionKey = new(_id);

            // Create the message
            await _container.CreateItemAsync<Message>(item: message, partitionKey: partitionKey);

            // Now, update the corresponding session with the message ID
            if (message.SessionId != null)
            {
                await AddMessageIdToSessionAsync(message.SessionId, message.MessageId);
            }

            return message.MessageId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }

    private async Task AddMessageIdToSessionAsync(string sessionId, string messageId)
    {
        try
        {
            // Retrieve the Session document
            Session session = await GetSessionAsync(sessionId);

            // Ensure the messages array is initialized
            session.Messages ??= new List<string>();

            // Add the new message ID to the messages array
            session.Messages.Add(messageId);

            // Update the Session document in Cosmos DB
            PartitionKey partitionKey = new(session.SessionId);
            await _container.ReplaceItemAsync(session, session.SessionId, partitionKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while updating Session: {ex.Message}");
            throw;
        }
    }

    public async Task<Session> GetSessionAsync(string sessionId)
    {
        try
        {
            ItemResponse<Session> response = await _container.ReadItemAsync<Session>(sessionId, new PartitionKey(sessionId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Session with ID '{sessionId}' not found.");
        }
    }

    public async Task<ChatUser> GetChatUserAsync(string chatuserId)
    {
        try
        {
            ItemResponse<ChatUser> response = await _container.ReadItemAsync<ChatUser>(chatuserId, new PartitionKey(chatuserId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"ChatUser with ID '{chatuserId}' not found.");
        }
    }

    public async Task<List<Session>> GetSessionsByChatUserIdAsync(string chatUserId)
    {

        ChatUser chatUser = await GetChatUserAsync(chatUserId);

        // If no sessions are found, return an empty list
        if (chatUser.Sessions == null || chatUser.Sessions.Count == 0)
        {
            return new List<Session>();
        }

        return chatUser.Sessions;
    }

    public async Task<List<Message>> GetMessagesBySessionIdAsync(string sessionId)
    {
        // Step 1: Get the session by sessionId
        Session session = await GetSessionAsync(sessionId);

        // Step 2: Prepare to collect messages
        List<Message> messages = new List<Message>();

        // Step 3: Fetch messages linked to the session
        if (session.Messages != null)
        {
            foreach (var messageId in session.Messages)
            {
                Message message = await GetMessageAsync(messageId);
                messages.Add(message);
            }
        }

        return messages;
    }

    public async Task<Message> GetMessageAsync(string messageId)
    {
        try
        {
            ItemResponse<Message> response = await _container.ReadItemAsync<Message>(messageId, new PartitionKey(messageId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Message with ID '{messageId}' not found.");
        }
    }

}

