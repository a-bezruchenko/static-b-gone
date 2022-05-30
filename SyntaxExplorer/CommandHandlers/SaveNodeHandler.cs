using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    class SaveNodeHandler : ICommandHandler
    {
        public InteractiveModel Model { get; init; }

        public async Task Launch(string arguments)
        {
            Model.SyntaxCrawler?.SaveCurrentNode(arguments);
        }
    }
}
