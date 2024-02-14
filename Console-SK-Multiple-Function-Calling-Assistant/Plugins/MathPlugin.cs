using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;

namespace Console_SK_Assistant.Plugins
{
    public class MathPlugin
    {

        [KernelFunction, Description("Add two numbers")]
        public static double Add(
        [Description("The first number to add")] double number1,
        [Description("The second number to add")] double number2
    )
        {
            return number1 + number2;
        }
       [KernelFunction, Description("Multiply two numbers. When increasing by a percentage, don't forget to add 1 to the percentage.")]
        public static double Multiply(
          [Description("The first number to multiply")] double number1,
          [Description("The second number to multiply")] double number2
        )
        {
            return number1 * number2;
        }
    }
}
