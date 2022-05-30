using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    class GoToChildHandler : ICommandHandler
    {
        public InteractiveModel Model { get; init; }

        public async Task Launch(string arguments)
        {
            bool moved = await Model.SyntaxCrawler?.GoToChild(arguments);
            if (!moved)
                Console.WriteLine("Could not move.");
        }
    }
}
