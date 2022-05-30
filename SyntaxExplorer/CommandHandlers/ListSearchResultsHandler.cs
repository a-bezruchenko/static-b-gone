using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    class ListSearchResultsHandler : ICommandHandler
    {
        public InteractiveModel Model { get; init; }

        public async Task Launch(string arguments)
        {
            if (Model.LastSearchResults == null || Model.LastSearchResults.Count() == 0)
            {
                Console.WriteLine("No search results.");
            }
            else
            {
                Console.WriteLine("{0} search results.", Model.LastSearchResults.Count());
                foreach (var result in Model.LastSearchResults)
                {
                    Console.WriteLine("{0}: {1}", result.Document.Name, result.Node.ToString());
                }
            }
        }
    }
}
