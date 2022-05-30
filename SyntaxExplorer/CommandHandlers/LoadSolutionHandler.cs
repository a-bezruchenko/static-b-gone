using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    class LoadSolutionHandler : ICommandHandler
    {
        public InteractiveModel Model { get; init; }

        public async Task Launch(string arguments)
        {
            if (arguments == "")
                arguments = Model.DefaultSolutionPath;
            else
                Model.DefaultSolutionPath = arguments;

            Model.SyntaxCrawler = await SyntaxCrawler.OpenSolution(arguments);
        }
    }
}
