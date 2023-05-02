﻿//  Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class MockInstantiationRewriter : RewriterBase
    {
        private readonly IFormatter _formatter;

        public MockInstantiationRewriter(IFormatter formatter)
        {
            _formatter = formatter;
        }

        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var trackedNodes =
                node.TrackNodes(node.DescendantNodesAndSelf()
                        .Where(IsLocalDeclarationStatementOrInvocationExpression),
                    CompilationId);

            var baseCallNode = VisitBaseNodeAs<LocalDeclarationStatementSyntax>(() => base.VisitLocalDeclarationStatement(trackedNodes));

            if (!IsRhinoMocksLocalDeclarationWithoutVarType(baseCallNode))
            {
                return baseCallNode;
            }

            return baseCallNode.WithDeclaration(
                baseCallNode.Declaration
                    .WithType(
                        MoqSyntaxFactory.VarType
                            .WithLeadingTrivia(baseCallNode.Declaration.Type.GetLeadingTrivia())
                            .WithTrailingTrivia(baseCallNode.Declaration.Type.GetTrailingTrivia())));
        }

        private bool IsRhinoMocksLocalDeclarationWithoutVarType(LocalDeclarationStatementSyntax node)
        {
            return node.GetOriginalNode(node, CompilationId)!.Declaration.Variables
                       .Any(
                           s => s.Initializer is { } initializer
                                && Model.GetSymbolInfo(initializer.Value).Symbol?.ContainingType is { } symbol
                                && RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals(symbol, SymbolEqualityComparer.Default))
                   && !node.Declaration.Type.IsEquivalentTo(MoqSyntaxFactory.VarType, false);
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var trackedNodes =
                node.TrackNodes(
                    node.DescendantNodesAndSelf()
                        .Where(IsLocalDeclarationStatementOrInvocationExpression),
                    CompilationId);
            var baseCallNode = (InvocationExpressionSyntax) base.VisitInvocationExpression(trackedNodes)!;

            var originalNode = baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!;
            var methodSymbol = Model.GetSymbolAs<IMethodSymbol>(originalNode)?.OriginalDefinition;
            if (methodSymbol is null)
            {
                return baseCallNode;
            }

            var rhinoMocksMethodGenericName = baseCallNode.GetFirstGenericNameOrDefault();
            var syntaxNodePair = rhinoMocksMethodGenericName is null
                ? GetDataFromMockWithoutGenericName(baseCallNode)
                : GetDataFromMockWithGenericName(baseCallNode, rhinoMocksMethodGenericName);

            if (!syntaxNodePair.IsValid)
            {
                return baseCallNode;
            }

            return methodSymbol switch
            {
                _ when RhinoMocksSymbols.AllGenerateMockAndStubSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => _formatter.Format(MoqSyntaxFactory.MockCreationExpression(syntaxNodePair!, syntaxNodePair))
                        .WithLeadingAndTrailingTriviaOfNode(baseCallNode),
                _ when RhinoMocksSymbols.AllPartialMockSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => _formatter.Format(MoqSyntaxFactory.PartialMockCreationExpression(syntaxNodePair!, syntaxNodePair))
                        .WithLeadingAndTrailingTriviaOfNode(baseCallNode),
                _ when RhinoMocksSymbols.AllStrictMockSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => _formatter.Format(MoqSyntaxFactory.StrictMockCreationExpression(syntaxNodePair!, syntaxNodePair))
                        .WithLeadingAndTrailingTriviaOfNode(baseCallNode),
                _ => baseCallNode
            };
        }

        private static bool IsLocalDeclarationStatementOrInvocationExpression(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.LocalDeclarationStatement) || node.IsKind(SyntaxKind.InvocationExpression);
        }

        private static SyntaxNodePair<TypeArgumentListSyntax?, ArgumentListSyntax?>GetDataFromMockWithGenericName(
            InvocationExpressionSyntax baseCallNode,
            GenericNameSyntax rhinoMocksMethodGenericName)
        {
            var moqMockTypeArgumentList = SyntaxFactory.TypeArgumentList()
                .AddArguments(
                    rhinoMocksMethodGenericName
                        .TypeArgumentList
                        .Arguments
                        .First());

            var moqMockArgumentSyntaxList = baseCallNode.ArgumentList;

            return (moqMockTypeArgumentList, moqMockArgumentSyntaxList);
        }

        private static SyntaxNodePair<TypeArgumentListSyntax?, ArgumentListSyntax?> GetDataFromMockWithoutGenericName(InvocationExpressionSyntax baseCallNode)
        {
            if (baseCallNode.ArgumentList.GetFirstArgumentOrDefault() is not { } typeArgument)
            {
                return (null, null);
            }

            var typeArgumentList = typeArgument.Expression switch
            {
                TypeOfExpressionSyntax {Type: { } type} => MoqSyntaxFactory.TypeArgumentList(type),
                TypeSyntax type => MoqSyntaxFactory.TypeArgumentList(type),
                _ => null
            };

            var argumentList = baseCallNode.ArgumentList
                .WithArguments(
                    SyntaxFactory.SeparatedList(
                        baseCallNode
                            .ArgumentList
                            .Arguments
                            .Skip(1)));

            return (typeArgumentList, argumentList);
        }
    }
}