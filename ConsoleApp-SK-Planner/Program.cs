// Important Note:  This project uses Microsoft.SemanticKernel.Planners.Handlebars 1.7.1 preview 
using Console_SK_Planner.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System.Configuration;
using System.Net;
using Microsoft.SemanticKernel.Planning;
// RDC: Working as of 4/21/24
#region Step 1:  Create Builder, Add AI ChatCompletion
var builder = Kernel.CreateBuilder();

var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");

if (openAiDeployment != null && openAiUri != null && openAiApiKey != null)
{
    builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: openAiDeployment,
    endpoint: openAiUri,
    serviceId: "AzureOpenAIChat",
    apiKey: openAiApiKey,
    modelId: "gpt-35-turbo-16k");

}
#endregion

#region Step 2:  Create Kernel, Load Plugins from Prompts
var kernel = builder.Build();
// Used for Debugging purposes
// string executingDirectory = Environment.CurrentDirectory;
// Load prompts
kernel.ImportPluginFromPromptDirectory(Path.Combine("./Plugins/Prompts", "SummarizePlugin"));
// If using Native Plugins you can load them like this... builder.Plugins.AddFromType<CheckHistoryPlugin>();
//builder.Plugins.AddFromType<LightOnPlugin>();
//builder.Plugins.AddFromType<WeatherPlugin>(); 
#endregion

#region Step 3:  Create Handlebar Planner Options
// Using Expermental features so we have to disable the warning
#pragma warning disable SKEXP0060 
// Set the planner options
HandlebarsPlannerOptions? plannerOptions = null;
plannerOptions ??= new HandlebarsPlannerOptions()
{
    // When using OpenAI models, we recommend using low values for temperature and top_p to minimize planner hallucinations.
    ExecutionSettings = new OpenAIPromptExecutionSettings()
    {
        Temperature = 0.0,
        TopP = 0.1,
    },
};
// Use gpt-4 or newer models if you want to test with loops.
// Older models like gpt-35-turbo are less recommended. They do handle loops but are more prone to syntax errors.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
plannerOptions.AllowLoops = openAiDeployment.Contains("gpt-4", StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#endregion

#region Step 4:  Instantiate the planner and create the plan
var planner = new HandlebarsPlanner(plannerOptions);
var goal = "Send Mary an email with the list of meetings I have scheduled today";
KernelArguments? initialContext = null;
// Following can be used to set the initialContext arguments
//var initialArguments = new KernelArguments()
//        {
//            { "greetings", new List<string>(){ "hey", "bye" } },
//            { "someNumber", 1 },
//            { "person", new Dictionary<string, string>()
//            {
//                {"name", "John Doe" },
//                { "language", "Italian" },
//            } }
//        };
var plan = await planner.CreatePlanAsync(kernel, goal, initialContext);
#endregion


#region Step 5: Set the Exercise to run and execute the plan.
// values can be 1 - 5;
var exercise_to_run = 3;


switch (exercise_to_run) {
    case 1: // Run Exercise 1 Send Email with invalid plan
        {
            // Let's try to create and run a plan that is not possible!
            await ExerciseSendEmailAsync(kernel, plan,"Exercise 1: Plan NOT possible!", initialContext);
            break;
        }
    case 2: // Run Exercise 2 Send Email with valid plan (include meeting details)
        {
            // Now, let's provide the meeting details
            goal = "Send Mary an email with the list of meetings I have scheduled today.  I have meetings a 9AM with Mayor Smith about Taxes, one at 10AM with Lead Developer, 1PM with CPO.";
            plan = await planner.CreatePlanAsync(kernel, goal, initialContext);
            await ExerciseSendEmailAsync(kernel, plan, "Exercise 1: Plan possible!", initialContext);
            break;
        }
    case 3: // Run Exercise 3 Write a Poem using two plugins
        {
            goal = "Write a poem about John Doe, then translate the result into Italian.";
            kernel.Plugins.Clear(); // Let's remove the plugins we added and add new ones
            kernel.ImportPluginFromPromptDirectory(Path.Combine("./Plugins/Prompts", "WriterPlugin"));
            kernel.ImportPluginFromPromptDirectory(Path.Combine("./Plugins/Prompts", "SummarizePlugin"));
            // Plugins must be added before the plan is created
            plan = await planner.CreatePlanAsync(kernel, goal, initialContext);
            await ExerciseWritePoemAsync(kernel, plan, "Exercise 3: Multiple Plugins", initialContext);
            break;
        }
    default:
        {
          
            Console.WriteLine("Number is not 1, 2, or 3");
            break;
        }
}
#endregion

#region Helper:  PrintPlannerDetails
void PrintPlannerDetails(string goal, HandlebarsPlan plan, string result, bool shouldPrintPrompt)
{
    Console.WriteLine($"Goal: {goal}");
    Console.WriteLine($"\nOriginal plan:\n{plan}");
    Console.WriteLine($"\nResult:\n{result}\n");

    // Print the prompt template
    if (shouldPrintPrompt && plan.Prompt is not null)
    {
        Console.WriteLine("\n\n======== CreatePlan Prompt ========");
        Console.WriteLine(plan.Prompt);
    }
}
#endregion

#region Helper:  ExerciseSendEmail
async Task ExerciseSendEmailAsync(Kernel kernel, HandlebarsPlan plan, string msg, KernelArguments? initialContext=null)
{
    // In this exercise we demostrate 
    Console.WriteLine(msg);
    try
    {
        bool shouldInvokePlan = true;
        var result = shouldInvokePlan ? await plan.InvokeAsync(kernel, initialContext) : string.Empty;
        PrintPlannerDetails(goal, plan, result, false);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.InnerException?.Message);
    }
}
#endregion

#region Helper:  Exercise Write a poem convert to Italian
async Task ExerciseWritePoemAsync(Kernel kernel, HandlebarsPlan plan, string msg, KernelArguments? initialContext = null)
{
    // In this exercise we demostrate 
    Console.WriteLine(msg);
    try
    {
        bool shouldInvokePlan = true;
        var result = shouldInvokePlan ? await plan.InvokeAsync(kernel, initialContext) : string.Empty;
        PrintPlannerDetails(goal, plan, result, true);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.InnerException?.Message);
    }
}
#endregion
