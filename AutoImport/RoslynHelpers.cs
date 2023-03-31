using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;

namespace AutoImport
{
    public static class RoslynHelpers
    {
        public static FieldDeclarationSyntax CreateFieldDeclaration(string type, string name)
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(type))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name)))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
        }

        public static Document RenameIdentifier(Document document, IdentifierNameSyntax typeDecl, string newPrivateName)
        {
            var syntaxTree = document.GetSyntaxRootAsync().Result;

            var newRoot = syntaxTree
                .ReplaceToken(typeDecl.Identifier, SyntaxFactory.Identifier(newPrivateName));

            return document.WithSyntaxRoot(newRoot);
        }

        public static Document InitializeProperty(Document document, string ctorPropname, string newPrivateName, string typeString)
        {
            var syntaxTree = document.GetSyntaxRootAsync().Result;

            var oldConstructor = syntaxTree.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .First();

            var newConstructor = oldConstructor
                            .WithBody(oldConstructor.Body.AddStatements(
                                 SyntaxFactory.ExpressionStatement(
                                 SyntaxFactory.AssignmentExpression(
                                 SyntaxKind.SimpleAssignmentExpression,
                                 SyntaxFactory.IdentifierName(newPrivateName),
                                 SyntaxFactory.IdentifierName(ctorPropname)))))

                            .WithParameterList(oldConstructor.ParameterList.AddParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(ctorPropname))
                                    .WithType(SyntaxFactory.ParseTypeName(typeString)
                                )));

            var oldClass = syntaxTree.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var oldClassWithNewCtor = oldClass.ReplaceNode(oldConstructor, newConstructor);

            var fieldDeclaration = RoslynHelpers.CreateFieldDeclaration(typeString, newPrivateName);

            var oldClassWithNewCtorAndField = oldClassWithNewCtor.WithMembers(
                  oldClassWithNewCtor.Members.Insert(0, fieldDeclaration))
                  .WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = syntaxTree.ReplaceNode(oldClass, oldClassWithNewCtorAndField);

            return document.WithSyntaxRoot(newRoot);
        }

        public static bool IsTypeInterface(SemanticModel semanticModel, IdentifierNameSyntax typeIdent)
        {
            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(typeIdent).Type;
            if (typeSymbol != null)
            {
                return typeSymbol.TypeKind == TypeKind.Interface;
            }
            return false;
        }
    }

}

