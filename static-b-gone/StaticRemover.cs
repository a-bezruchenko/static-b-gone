using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace static_b_gone
{
    public class StaticRemover
    {
        public string PathToProject { get; set; }
        public string ClassToReplace { get; set; }
        private string InterfaceNameToReplaceWith => "I" + ClassToReplace;
        private string FieldNameToReplaceWith => "_" + Char.ToLower(ClassToReplace[0]) + ClassToReplace.Substring(1);

        public async Task RemoveStatic()
        {
            /*var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1 ? visualStudioInstances[0] : SelectVisualStudioInstance(visualStudioInstances);
            MSBuildLocator.RegisterInstance(instance);*/
            MSBuildLocator.RegisterDefaults();

            Stopwatch sw = Stopwatch.StartNew();

            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solution = await workspace.OpenSolutionAsync(PathToProject);
                var projectIds = solution.ProjectIds;

                foreach (var projectId in projectIds)
                {
                    var project = solution.GetProject(projectId);
                    var documentIds = project.DocumentIds;

                    foreach (var documentId in documentIds)
                    {
                        var document = project.GetDocument(documentId);
                        document = await RemoveStaticFromFile(workspace, document);
                        project = document.Project;
                    }

                    solution = project.Solution;
                }

                workspace.TryApplyChanges(solution);
            }

            sw.Stop();

            Console.WriteLine("Finished in {0} seconds", sw.ElapsedMilliseconds / 1000);
        }        

        private async Task<Document> RemoveStaticFromFile(MSBuildWorkspace workspace, Document file)
        {
            var syntaxTree = await file.GetSyntaxTreeAsync();
            var root = syntaxTree.GetRoot();

            var occurencesToReplace = root.DescendantNodes().OfType<IdentifierNameSyntax>().Where(x => x.ToString() == ClassToReplace).ToList();
            if (occurencesToReplace.Count > 0)
            {
                bool hasStatic = false;
                bool hasNonStatic = false;
                foreach (var node in occurencesToReplace)
                {
                    if (IsNodeInStaticFunc(node))
                        hasStatic = true;
                    else
                        hasNonStatic = true;
                }

                var editor = await DocumentEditor.CreateAsync(file);

                if (hasStatic)
                {
                    var usingDiNode = GetUsingDiNode();
                    var lastUsingNode = GetLastUsingNode(root);
                    if (lastUsingNode != null)
                        editor.InsertAfter(lastUsingNode, usingDiNode);
                }

                if (hasNonStatic)
                {

                }

                
                foreach (var node in occurencesToReplace)
                {
                    // replace it with call to ServiceLocator
                    editor.ReplaceNode(
                        node,
                        GetServiceLocatorCallNode(InterfaceNameToReplaceWith)
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia()));

                    /*
                    if (IsNodeInStaticFunc(node))
                    {
                        // replace it with call to ServiceLocator
                    }
                    else
                    {
                        // replace it with call to field
                    }
                    */
                }

                file = editor.GetChangedDocument();
            }

            return file;
        }

        private SyntaxNode ReadNodeFromFile(string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                return CSharpSyntaxNode.DeserializeFrom(fs);
            }
        }

        private SyntaxNode GetLastUsingNode(SyntaxNode root)
        {
            return root.DescendantNodes().OfType<UsingDirectiveSyntax>().LastOrDefault();
        }

        private VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        SyntaxNode _serviceLocatorCallNode;

        private SyntaxNode GetServiceLocatorCallNode(string className)
        {
            if (_serviceLocatorCallNode == null)
            {
                _serviceLocatorCallNode = ReadNodeFromFile("service_locator_call.bin");
            }

            return _serviceLocatorCallNode;
        }

        SyntaxNode _usingDiNode;

        private SyntaxNode GetUsingDiNode()
        {
            if (_usingDiNode == null)
            {
                _usingDiNode = ReadNodeFromFile("using_di.bin");
            }

            return _usingDiNode;
        }

        // check if node belongs to static method, field or member
        private bool IsNodeInStaticFunc(SyntaxNode node)
        {
            return IsNodeInStaticProperty(node) || IsNodeInStaticMethod(node);
        }

        private bool IsNodeInStaticProperty(SyntaxNode node)
        {
            PropertyDeclarationSyntax propertyNode;
            while (node != null && node is not PropertyDeclarationSyntax)
                node = node.Parent;

            if (node == null)
                return false;

            propertyNode = (PropertyDeclarationSyntax)node;
            return propertyNode.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        private bool IsNodeInStaticMethod(SyntaxNode node)
        {
            MethodDeclarationSyntax propertyNode;
            while (node != null && node is not MethodDeclarationSyntax)
                node = node.Parent;

            if (node == null)
                return false;

            propertyNode = (MethodDeclarationSyntax)node;
            return propertyNode.Modifiers.Any(SyntaxKind.StaticKeyword);
        }
    }
}
