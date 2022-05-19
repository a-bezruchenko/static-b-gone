using System;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using static_b_gone.Util;

namespace static_b_gone
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var parsedArgs = new CmdLineParser().ParseArgs(args);
            //var configArgs;
            //const string path = "C:\\projects\\ramplus\\RamPlusSQL.sln";

            string path = parsedArgs["path"];
            string classToReplace = parsedArgs["class"];

            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(path);
            foreach (var project in solution.Projects)
            {
                foreach (var file in project.Documents)
                {
                    if (file.Name.Contains(classToReplace))
                        Console.WriteLine(file.Name);
                }
            }

        }
    }
}
