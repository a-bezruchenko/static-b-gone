using System;
using System.Threading.Tasks;
using static_b_gone.Util;

namespace static_b_gone
{
    class Program
    {
        const string configFileName = "config.json";
        public static async Task Main(string[] args)
        {
            var parsedArgs = new CmdLineParser().ParseArgs(args);
            var configArgs = new ConfigReader().ReadConfig(configFileName);

            string path = parsedArgs?.GetValueOrDefault("path") ?? configArgs?.GetValueOrDefault("path") ?? throw new ArgumentNullException("Need to specify path to project file");
            string classToReplace = parsedArgs?.GetValueOrDefault("class") ?? configArgs?.GetValueOrDefault("class") ?? throw new ArgumentNullException("Need to specify class to replace");

            var staticRemover = new StaticRemover();
            await staticRemover.RemoveStatic(path, classToReplace);
        }
    }
}
