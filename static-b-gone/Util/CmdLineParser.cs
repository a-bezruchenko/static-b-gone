using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace static_b_gone.Util
{
    public class CmdLineParser
    {
        private readonly string[] argNamePrefixes = { "--", "-", "/" };
        private const string emptyValueFiller = "";

        public Dictionary<string, string> ParseArgs(string[] args)
        {
            bool prevIsName = false;
            string prevValue = "";
            Dictionary<string, string> result = new Dictionary<string, string>();
            bool parsedSuccessfully = true;

            foreach (var arg in args)
            {
                if (IsArgName(arg))
                {
                    if (prevIsName)
                    {
                        result[prevValue] = emptyValueFiller;
                    }

                    prevIsName = true;
                    prevValue = RemoveArgNamePrefix(arg.ToLower());
                }
                else
                {
                    if (prevIsName)
                    {
                        result[prevValue] = arg;
                        prevIsName = false;
                    }
                    else
                    {
                        parsedSuccessfully = false;
                        break;
                    }
                }
            }

            if (parsedSuccessfully)
                return result;
            else
                return null;
        }

        private bool IsArgName(string arg)
        {
            foreach (var start in argNamePrefixes)
            {
                if (arg.StartsWith(start))
                    return true;
            }
            return false;
        }

        private string RemoveArgNamePrefix(string arg)
        {
            foreach (var start in argNamePrefixes)
            {
                if (arg.StartsWith(start))
                    return arg.Replace(start, null);
            }
            return arg;
        }
    }
}
