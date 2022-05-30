using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace SyntaxTreeExplorer
{
    enum CurrentSyntaxLevel
    {
        SolutionSelected,
        ProjectSelected,
        FileSelected,
        NodeSelected
    }

    public class SyntaxCrawler : IDisposable
    {
        public static async Task<SyntaxCrawler> OpenSolution(string pathToSolution)
        {
            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);
            var solution = await workspace.OpenSolutionAsync(pathToSolution);
            return new SyntaxCrawler
            {
                CurrentSolution = solution,
                CurrentProject = null,
                CurrentDocument = null,
                CurrentNode = null,
                CurrentSyntaxLevel = CurrentSyntaxLevel.SolutionSelected,
                Workspace = workspace,
            };
        }

        private Solution CurrentSolution { get; init; }
        private Project CurrentProject { get; set; }
        private Document CurrentDocument { get; set; }
        private SyntaxNode CurrentNode { get; set; }
        private CurrentSyntaxLevel CurrentSyntaxLevel { get; set; }

        private MSBuildWorkspace Workspace { get; init; }

        public void GoToParent()
        {
            if (CurrentSyntaxLevel == CurrentSyntaxLevel.SolutionSelected)
                return;
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.ProjectSelected)
            {
                CurrentSyntaxLevel = CurrentSyntaxLevel.SolutionSelected;
                CurrentProject = null;
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.FileSelected)
            {
                CurrentSyntaxLevel = CurrentSyntaxLevel.ProjectSelected;
                CurrentDocument = null;
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.NodeSelected)
            {
                if (CurrentNode.Parent == null)
                {
                    CurrentSyntaxLevel = CurrentSyntaxLevel.FileSelected;
                    CurrentNode = null;
                }
                else
                {
                    CurrentNode = CurrentNode.Parent;
                }
            }
        }

        public async Task ListChildren()
        {
            if (CurrentSyntaxLevel == CurrentSyntaxLevel.SolutionSelected)
            {
                foreach (var project in CurrentSolution.Projects)
                {
                    PrintProjectInfo(project);
                }
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.ProjectSelected)
            {
                foreach (var document in CurrentProject.Documents)
                {
                    PrintDocumentInfo(document);
                }
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.FileSelected)
            {
                PrintNodeInfo(await CurrentDocument.GetSyntaxRootAsync(), 0);
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.NodeSelected)
            {
                foreach (var (node, i) in CurrentNode.ChildNodes().Select((value, i) => (value, i)))
                {
                    PrintNodeInfo(node, i);
                }
            }
        }

        public async Task<bool> GoToChild(string idOrName)
        {
            bool moved = false;
            if (CurrentSyntaxLevel == CurrentSyntaxLevel.SolutionSelected)
            {
                Project project = null;
                try
                {
                    project = CurrentSolution.Projects.Where(x => x.Id.Id.ToString() == idOrName)
                        .Union(CurrentSolution.Projects.Where(x => x.Name.ToString() == idOrName)).SingleOrDefault();
                }
                catch (InvalidOperationException)
                {
                    project = null;
                }

                CurrentProject = project;

                if (CurrentProject != null)
                {
                    CurrentSyntaxLevel = CurrentSyntaxLevel.ProjectSelected;
                    moved = true;
                }
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.ProjectSelected)
            {
                Document doc = null;
                try
                {
                    doc = CurrentProject.Documents.Where(x => x.Id.Id.ToString() == idOrName)
                        .Union(CurrentProject.Documents.Where(x => x.Name.ToString() == idOrName)).SingleOrDefault();
                }
                catch (InvalidOperationException)
                {
                    doc = null;
                }

                CurrentDocument = doc;
                if (CurrentDocument != null)
                {
                    CurrentSyntaxLevel = CurrentSyntaxLevel.FileSelected;
                    moved = true;
                }
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.FileSelected)
            {
                CurrentSyntaxLevel = CurrentSyntaxLevel.NodeSelected;
                CurrentNode = await CurrentDocument.GetSyntaxRootAsync();
                moved = true;
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.NodeSelected)
            {
                var node = CurrentNode.ChildNodes().Skip(int.Parse(idOrName)).FirstOrDefault();
                if (node != null)
                {
                    CurrentNode = node;
                    moved = true;
                }
            }
            return moved;
        }

        public async Task GoToNode(SyntaxNode node, Document document)
        {
            
            CurrentNode = node;
            CurrentDocument = document;
            CurrentProject = document.Project;


        }

        public void SaveCurrentNode(string filepath)
        {
            if (CurrentNode != null)
            {
                SaveNodeToFile(CurrentNode, filepath);
            }
        }

        private void PrintDocumentInfo(Document doc)
        {
            Console.WriteLine("{0} {1}", doc.Id, doc.Name);
        }

        private void PrintProjectInfo(Project project)
        {
            Console.WriteLine("{0} {1}", project.Id, project.Name);
        }

        private void PrintNodeInfo(SyntaxNode node, int index)
        {
            Console.WriteLine("{0} {1} {2}", index, node.Kind().ToString(), node.ToString());
        }

        public async Task<IEnumerable<SearchResult>> FindAndGetElements(string elementText, int limit = 1)
        {
            if (CurrentSyntaxLevel == CurrentSyntaxLevel.SolutionSelected)
            {
                return await FindElements(CurrentSolution, elementText, limit);
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.ProjectSelected)
            {
                return await FindElements(CurrentProject, elementText, limit);
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.FileSelected)
            {
                return await FindElements(CurrentDocument, elementText, limit);
            }
            else if (CurrentSyntaxLevel == CurrentSyntaxLevel.NodeSelected)
            {
                return FindElementsInNodeChildren(CurrentNode, CurrentDocument, elementText, limit);
            }
            else
            {
                throw new InvalidOperationException("crawler is in invalid state");
            }
        }

        private async Task<IEnumerable<SearchResult>> FindElements(Solution parentSolution, string elementText, int limit = 1)
        {
            List<SearchResult> result = new();
            foreach (var project in parentSolution.Projects)
            {
                var res = await FindElements(project, elementText, limit);
                result.AddRange(res);
                limit -= res.Count();
                if (limit <= 0)
                    break;
            }
            return result;
        }

        private async Task<IEnumerable<SearchResult>> FindElements(Project parentProject, string elementText, int limit = 1)
        {
            List<SearchResult> result = new();
            foreach (var document in parentProject.Documents)
            {
                var res = await FindElements(document, elementText, limit);
                result.AddRange(res);
                limit -= res.Count();
                if (limit <= 0)
                    break;
            }
            return result;
        }

        private async Task<IEnumerable<SearchResult>> FindElements(Document parentDocument, string elementText, int limit = 1)
        {
            return FindElementsInNodeChildren(await parentDocument.GetSyntaxRootAsync(), parentDocument, elementText, limit);
        }

        private IEnumerable<SearchResult> FindElementsInNodeChildren(SyntaxNode parentNode, Document parentDocument, string elementText, int limit = 1)
        {
            if (limit <= 0)
                return Enumerable.Empty<SearchResult>();

            List<SearchResult> result = new();

            var foundNodes = CurrentNode.DescendantNodes().Where(x => x.ToString() == elementText).ToList();
            if (foundNodes.Count > 0)
            {
                foreach (var node in foundNodes)
                {
                    result.Add(new SearchResult() { Node = node, Document = parentDocument });
                    limit--;
                    if (limit <= 0)
                        return result;
                }
            }

            return result;
        }

        public void PrintChildrenTreeFromCurrentNode(bool showTokens = true)
        {
            if (CurrentNode != null)
                PrintTree(CurrentNode, showTokens: showTokens);
        }

        private void PrintTree(SyntaxNodeOrToken root, int depth = 0, bool showTokens = true)
        {
            string indent = new string(' ', depth * 4);
            Console.WriteLine("{0}{1} {2}", indent, root.Kind().ToString(), root.ToString());
            if (root.IsNode)
            {
                foreach (var child in root.ChildNodesAndTokens())
                {
                    if (child.IsNode || (child.IsToken && showTokens))
                        PrintTree(child, depth + 1);
                }
            }
        }

        private void SaveNodeToFile(SyntaxNode node, string filepath)
        {
            using (FileStream fs = File.OpenWrite(filepath))
                node.SerializeTo(fs);
        }

        public void Dispose()
        {
            Workspace.Dispose();
        }
    }
}
