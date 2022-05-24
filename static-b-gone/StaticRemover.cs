using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace static_b_gone
{
    public class StaticRemover
    {
        public string PathToProject { get; set; }
        public string ClassToReplace { get; set; }
        private string InterfaceNameToReplaceWith => GetInterfaceNameForClassReplacement(ClassToReplace);
        private string FieldNameToReplaceWith => GetFieldNameForClassReplacement(ClassToReplace);

        public async Task RemoveStatic()
        {
            //var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            //var instance = visualStudioInstances[0];
            MSBuildLocator.RegisterDefaults();

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
        }        

        private async Task<Document> RemoveStaticFromFile(MSBuildWorkspace workspace, Document file)
        {
            var syntaxTree = await file.GetSyntaxTreeAsync();
            var root = syntaxTree.GetRoot();

            var occurencesToReplace = root.DescendantNodes().OfType<IdentifierNameSyntax>().Where(x => x.ToString() == ClassToReplace).ToList();
            if (occurencesToReplace.Count > 0)
            {
                var editor = await DocumentEditor.CreateAsync(file);
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
                        // check if we have field for this class, create if needed
                        // replace it with call to field
                    }
                    */
                }

                file = editor.GetChangedDocument();
            }

            return file;
        }

        private void PrintTree(SyntaxNodeOrToken root, int depth = 0)
        {
            string indent = new string(' ', depth * 4);
            Console.WriteLine("{0}{1} {2}", indent, root.Kind().ToString(), root.ToString());
            if (root.IsNode)
            {
                foreach (var child in root.ChildNodesAndTokens())
                {
                    PrintTree(child, depth + 1);
                }
            }
        }

        SyntaxNode _serviceLocatorCallNode;

        private SyntaxNode GetServiceLocatorCallNode(string className)
        {
            if (_serviceLocatorCallNode == null)
            {
                /*
                InvocationExpression ServiceLocator.Instance.Resolve<IClass>()
                    SimpleMemberAccessExpression ServiceLocator.Instance.Resolve<IClass>
                        SimpleMemberAccessExpression ServiceLocator.Instance
                            IdentifierName ServiceLocator
                            IdentifierName Instance
                        GenericName Resolve<IClass>
                            TypeArgumentList <IClass>
                                IdentifierName IClass
                    ArgumentList ()
                */

                _serviceLocatorCallNode = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("ServiceLocator"),
                            SyntaxFactory.IdentifierName("Instance")
                            ),
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("Resolve"),
                            SyntaxFactory.TypeArgumentList(
                                new SeparatedSyntaxList<TypeSyntax>().Add(
                                    SyntaxFactory.IdentifierName(className))
                                )
                            )
                        ),
                    SyntaxFactory.ArgumentList()
                    );
            }

            return _serviceLocatorCallNode;
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

        private string GetFieldNameForClassReplacement(string targetClassName)
        {
            return "_" + Char.ToLower(targetClassName[0]) + targetClassName.Substring(1);
        }

        private string GetInterfaceNameForClassReplacement(string targetClassName)
        {
            return "I" + targetClassName;
        }

    }
}
