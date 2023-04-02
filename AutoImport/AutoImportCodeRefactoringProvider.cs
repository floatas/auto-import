using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoImport
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutoImportCodeRefactoringProvider)), Shared]
    internal class AutoImportCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var node = root.FindNode(context.Span);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            CreateDataModel(node, semanticModel, out var dataModel, out var typeDecl, out var binaryExpression);

            if (string.IsNullOrEmpty(dataModel.NewName))
            {
                return;
            }

            var action = CodeAction.Create("Introduce into constructor", c =>
                {
                    Document modifiedDocument = Refactor(context, dataModel, typeDecl, binaryExpression);

                    return Task.FromResult(modifiedDocument.Project.Solution);
                }
            );

            context.RegisterRefactoring(action);
        }

        private Document Refactor(CodeRefactoringContext context, DataModel dataModel, IdentifierNameSyntax typeDecl, BinaryExpressionSyntax binaryExpression)
        {
            var firstLetterIndex = dataModel.IsInterface ? 1 : 0;

            var ctorPropname = $"{char.ToLowerInvariant(dataModel.NewName[firstLetterIndex])}{dataModel.NewName.Substring(firstLetterIndex + 1)}";
            var newPrivateName = $"_{ctorPropname}";
            var typeString = dataModel.TypeName;

            var modifiedDocument = context.Document;

            if (typeDecl != null)
            {
                modifiedDocument = RoslynHelpers.RenameIdentifier(context.Document, typeDecl, newPrivateName);
            }

            if (dataModel.IsGeneric)
            {
                modifiedDocument = RenameGeneric(context.Document, binaryExpression, newPrivateName);
            }

            modifiedDocument = RoslynHelpers.InitializeProperty(modifiedDocument, ctorPropname, newPrivateName, typeString);
            return modifiedDocument;
        }

        private static void CreateDataModel(SyntaxNode node, SemanticModel semanticModel, out DataModel dataModel,
            out IdentifierNameSyntax typeDecl,
            out BinaryExpressionSyntax binaryExpression)
        {
            dataModel = new DataModel();

            typeDecl = null;
            binaryExpression = null;

            switch (node)
            {
                case IdentifierNameSyntax identifier:
                    typeDecl = identifier;
                    dataModel.NewName = StringHelpers.RemoveSpecialCharacters(typeDecl.Identifier.Text);
                    dataModel.TypeName = StringHelpers.RemoveSpecialCharacters(dataModel.NewName);
                    dataModel.IsInterface = RoslynHelpers.IsTypeInterface(semanticModel, typeDecl);
                    break;
                case BinaryExpressionSyntax expression:
                    binaryExpression = expression;
                    var regex = new Regex("(?<type>.*?)<", RegexOptions.Compiled);
                    dataModel.TypeName = StringHelpers.RemoveSpecialCharacters(binaryExpression.ToString());

                    if (regex.IsMatch(dataModel.TypeName))
                    {
                        dataModel.NewName = StringHelpers.RemoveSpecialCharacters(regex.Match(dataModel.TypeName).Groups["type"].Value);

                        dataModel.IsGeneric = true;
                    }

                    dataModel.IsInterface = dataModel.TypeName[0] == 'I';
                    break;
            }
        }

        private Document RenameGeneric(Document document, BinaryExpressionSyntax binaryExpression, string newPrivateName)
        {
            var syntaxTree = document.GetSyntaxRootAsync().Result;

            var newNode = SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(newPrivateName));

            var newRoot = syntaxTree
                .ReplaceNode(binaryExpression,
                newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}

