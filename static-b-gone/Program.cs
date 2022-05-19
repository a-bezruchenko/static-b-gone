using System;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace static_b_gone
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            const string path = "C:\\projects\\ramplus\\RamPlusSQL.sln";

            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(path);
            foreach (var project in solution.Projects)
            {
                foreach (var file in project.Documents)
                {
                    Console.WriteLine(file.Name);
                }
            }

        }
    }
}
