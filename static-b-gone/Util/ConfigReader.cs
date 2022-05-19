using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace static_b_gone.Util
{
    public class ConfigReader
    {
        public Dictionary<string, string> ReadConfig(string filename)
        {
            Dictionary<string, string> result;
            if (!File.Exists(filename))
            {
                result = new Dictionary<string, string>();
                File.WriteAllText(filename, JsonSerializer.Serialize(result));
            }
            else
            {
                result = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(filename));
            }

            return result;
        }
    }
}
