using SyntaxTreeExplorer;
using System;
using System.Threading.Tasks;

namespace SyntaxExplorer
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var controller = new InteractiveController();
            await controller.Start();
        }
    }
}
