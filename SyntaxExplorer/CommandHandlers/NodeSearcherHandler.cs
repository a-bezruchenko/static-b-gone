using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    class NodeSearcherHandler : ICommandHandler
    {
        public InteractiveModel Model { get; init; }

        public async Task Launch(string arguments)
        {
            Model.LastSearchResults = await Model.SyntaxCrawler?.FindAndGetElements(arguments);
            Console.WriteLine("Found {0} results", Model.LastSearchResults.Count());
        }
    }
}
