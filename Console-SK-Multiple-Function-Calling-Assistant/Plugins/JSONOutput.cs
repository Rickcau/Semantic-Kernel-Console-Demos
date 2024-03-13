using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Console_SK_Assistant.Plugins
{
    public class JSONOutputPlugin
    {

        private static bool _lightOn;
        public Kernel _kernel;

        private string _jsonPrompt = @"USING THE STRUCTURE ENCLOSED IN <SCHEMA> USE THE INPUT AN CONSTRUCT JSON THAT HAS THE PROPERTIES SET BASED ON WHAT THE USER IS ASKING FOR
               <SCHEMA>
               reportPatientType:INPATIENT,
               ahrqVersion: V2023,
               riskAdjustmentModel: AMC_2022,
               timePeriod: {
                 by: QUARTER
                  },
              focusHospital: 123456
              <SCHEMA>
              
              OUTPUT MUST BE:
              - JSON ONLY

              User: {{$input}}
              Assistant:
               ";

        private string _prompt3 = @"SUMMARIZE THE QUERY REFERENCED IN THE USER SECTION BELOW IN 10 BULLET POINTS OR LESS

            SUMMARY MUST BE:
            - WORKPLACE / FAMILY SAFE NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY
            - G RATED
            - IF THERE ARE ANY  INCONSISTENCIES, DO YOUR BEST TO CALL THOSE OUT
            
            RESULTS:
            - IF THE RESULTS FROM THE AzureAISearchPlugin are not related to the query simply state you do not have enough information

            User:{{DBQueryPlugin.GetHRContact $query}}

            Assistant: ";

        public JSONOutputPlugin(Kernel kernel)
        {
            this._kernel = kernel;
        }

        [KernelFunction]
        [Description("A function that generates JSON with the properties set to what the user is asking for")]
        public async Task<string> GenerateJSON([Description("The filter and properities for the AI to use to construct the JSON")] string request)
        {
            var theFunction = _kernel.CreateFunctionFromPrompt(_jsonPrompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });

            var response = await _kernel.InvokeAsync(theFunction);
            // new() { ["input"] = input }
            return response.ToString();
        }

        [KernelFunction]
        [Description("Turn the light on or off ")]
        public static bool TurnLightOff([Description("Turn light on or off")] bool lightStatus)
        {
            if (lightStatus == true)
            {
                _lightOn = true;
            }
            else if (lightStatus == false)
            {
                _lightOn = false;
            }
            return _lightOn;
        }
    }
}
