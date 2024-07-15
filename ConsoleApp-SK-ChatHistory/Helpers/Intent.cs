using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;


namespace SKTraining.Helpers
{
    internal static class Intent
    {
        // This function is a very powerful Intent helper, it allows you to detect the intent and take action accordingly which 
        // is much more efficient, reduces the number of tokens consumed and allows you to avoid unnecesscary calls to your AI search or the LLM
        // I have seen other attempts at this, but this approach is very powerful and you are letting the LLM attempt to detect the Intent 3 times
        // we then take the quorum of the 3 results and use that for the intent
        public static async Task<string> GetIntent(IChatCompletionService chat, string query)
        {
            // Keep the ChatHistory local since we only need it to detect the Intent
            ChatHistory chatHistory = new ChatHistory();
            var whatistheintent = "not_found"; // default
            chatHistory.AddSystemMessage($@"Return the intent of the user.The intent must be one of the following strings:
                    - dbquery: Use this intent to answer questions about categories, moniker, db_id or db_name queries.
                    - not_found: Use this intent if you can't find a suitable answer
            
                    [Examples for dbquery type of questions]
                    User question: Can you give me the db_name for db_id 1009410?
                    Intent: dbquery
                    User question: Give me the moniker for Twentieth-Century Drama
                    Intent: dbquery
                    User question: Categorize the db_names into different topics
                    Intent: dbquery

                    Per user query what is the Intent?
                    Intent:");

            chatHistory.AddUserMessage("What is the moniker for db_id 2 ?");
            chatHistory.AddAssistantMessage("dbquery");
            chatHistory.AddUserMessage("What is the db_id for House of Commons?");
            chatHistory.AddAssistantMessage("dbquery");
            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                ResultsPerPrompt = 3, // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
            };
            try
            {
                // Call the ChatCompletion assking for 3 rounds to attempt to identify that intent
                var result = await chat.GetChatMessageContentsAsync(
                        chatHistory,
                        executionSettings);

                string threeturnresult = string.Join(", ", result.Select(o => o.ToString()));
                // Now we use Regex and Linq to find the intent that is repeated the most
                var words = Regex.Split(threeturnresult.ToLower(), @"\W+")
                      .Where(w => w.Length >= 3)
                      .GroupBy(w => w)
                      .OrderByDescending(g => g.Count())
                      .First();
                whatistheintent = words.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return whatistheintent;
        }
    }
}

