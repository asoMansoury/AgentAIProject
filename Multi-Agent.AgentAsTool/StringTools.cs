using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgent.ManualViaStructuredOutput
{
    public static class StringTools
    {
        public static string Uppercase(string input)
        {
            return input.ToUpper();
        }

        public static string Lowercase(string input)
        {
            return input.ToLower();
        }

        public static string Reverse(string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }
    }
}
