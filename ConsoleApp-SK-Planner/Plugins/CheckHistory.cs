using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Console_SK_Planner.Plugins
{
    public class CheckHistoryPlugin
    {
     
        const string someVar= "Placeholder for whatever";

        [KernelFunction]
        [Description("This function is used to search the history object from the main code.  You should pass in the history object when calling this function")]
        public static string SearchHistory([Description("The string to use to search the chat history")] string query)
        {
            var dummyPlaceholder = $"The content: {query} was not found in the ChatHistory!";
            return dummyPlaceholder;
        }
    }
}
