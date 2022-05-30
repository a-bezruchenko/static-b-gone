using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer.CommandHandlers
{
    public interface ICommandHandler
    {
        InteractiveModel Model { get; init; }
        Task Launch(string arguments);
    }
}
