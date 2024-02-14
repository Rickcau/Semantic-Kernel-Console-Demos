using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;

namespace Console_SK_Assistant.Plugins
{
    public class LightOnPlugin
    {

        private static bool _lightOn; 

        [KernelFunction]
        [Description("Is the light on or off?")]
        public static bool CheckLight([Description("Check is the light on or off")] string query)
        {
            if (_lightOn)
            {
                return true;
            }
            else
            {
                return false;
            }
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
