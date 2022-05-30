using SyntaxTreeExplorer.CommandHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer
{
    public class InteractiveController
    {
        readonly InteractiveModel _model;
        readonly Dictionary<string, ICommandHandler> _handlers;

        readonly ICommandHandler _wrongInputHandler;

        public InteractiveController()
        {
            _model = new InteractiveModel();

            _handlers = new Dictionary<string, ICommandHandler>
            {
                { "help", new HelpHandler(){ Model = _model} },
                { "load", new LoadSolutionHandler(){ Model = _model} },
                { "gotoparent", new GoToParentHandler(){ Model = _model} },
                { "parent", new GoToParentHandler(){ Model = _model} },
                { "gotochild", new GoToChildHandler(){ Model = _model} },
                { "goto", new GoToChildHandler(){ Model = _model} },
                { "listchildren", new ListChildrenHandler(){ Model = _model} },
                { "list", new ListChildrenHandler(){ Model = _model} },
                { "savenode", new SaveNodeHandler(){ Model = _model} },
                { "save", new SaveNodeHandler(){ Model = _model} },
                { "find", new NodeSearcherHandler(){ Model = _model} },
                { "showresults", new ListSearchResultsHandler(){ Model = _model} },
                { "printtree", new PrintTreeHandler(){ Model = _model} },
            };

            _wrongInputHandler = _handlers["help"];
        }

        public async Task Start()
        {
            while (true)
            {
                Console.Write(">");
                string input = Console.ReadLine();

                int commandArgumentsSeparatorPosition = input.IndexOf(' ');

                string command, arguments;
                if (commandArgumentsSeparatorPosition == -1)
                {
                    command = input;
                    arguments = "";
                }
                else
                {
                    command = input[..commandArgumentsSeparatorPosition];
                    arguments = input[(commandArgumentsSeparatorPosition + 1)..];
                }

                command = command.ToLower();

                if (_handlers.ContainsKey(command))
                {
                    await _handlers[command].Launch(arguments);
                }
                else
                {
                    await _wrongInputHandler.Launch(arguments);
                }
            }
        }
    }
}
