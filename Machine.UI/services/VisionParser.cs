using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services
{
    public static class VisionParser
    {
        public static List<string> Parse(string msg)
        {
            List<string> results = new List<string>();

            foreach (char c in msg)
            {
                switch (c)
                {
                    case '1':
                        results.Add("OK");
                        break;
                    case '0':
                        results.Add("NG");
                        break;
                    case '2':
                        results.Add("NONE");
                        break;
                    default:
                        results.Add("UNKNOWN");
                        break;
                }
            }

            return results;
        }
    }
}
