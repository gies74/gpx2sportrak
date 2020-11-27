using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Magellan
{
    internal static class Utils
    {

        public static string CS(string inString)
        {
            inString = inString.Replace("\r", "");
            var pat = @"^\$([A-Za-z\d,. ]+)\*([\dA-F]{2})?$";
            var payload = Regex.Replace(inString, pat, "$1");
            var ba = System.Text.Encoding.UTF8.GetBytes(payload);
            byte b = 0x00;
            foreach (var be in ba)
            {
                b ^= be;
            }
            var cs = b.ToString("X2");
            var sum = Regex.Replace(inString, pat, "$2");
            if (sum != "" && sum != cs)
            {
                throw new Exception("Wrong sum");
            }
            return $"${payload}*{cs}" + Environment.NewLine;
        }
    }
}
