using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxTreeExplorer
{
    public class SearchResult
    {
        public SyntaxNode Node { get; set; }
        public Document Document { get; set; }
    }
}
