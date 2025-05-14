using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarParser
{
    public class DebugLog
    {
        public static void UpdateUI(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] " + message);
        }
    }
}
