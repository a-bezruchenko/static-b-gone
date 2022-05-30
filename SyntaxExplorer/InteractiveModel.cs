using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer
{
    public class InteractiveModel
    {
        const string configPath = "config.txt";

        public SyntaxCrawler SyntaxCrawler { get; set; }
        public IEnumerable<SearchResult> LastSearchResults { get; set; }
        public string DefaultSolutionPath
        {
            get 
            {
                if (File.Exists(configPath))
                    return File.ReadAllText(configPath);
                else
                    return "";
            }

            set => File.WriteAllText(configPath, value);
        }
    }
}
