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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class OrderedMockRewriter : RewriterBase
    {
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            MethodDeclarationSyntax treeWithTrackedNodes;
            try
            {
                treeWithTrackedNodes = node.Track(node.DescendantNodesAndSelf(), CompilationId);
            }
            catch (Exception)
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to convert node\r\n"
                    + $"  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");
                return node;
            }

            var usingStatements = GetRhinoMocksOrderedUsingStatements(treeWithTrackedNodes).ToList();
            for (var i = 0; i < usingStatements.Count; i++)
            {
                var usingStatement = usingStatements[i];
                var trackedUsingStatement = treeWithTrackedNodes.GetCurrent(usingStatement, CompilationId);
                int? index = usingStatements.Count == 1 ? null : i + 1;
                var statements = ReplaceExpressionStatements(((BlockSyntax)usingStatement.Statement).Statements, index);

                try
                {
                    treeWithTrackedNodes = treeWithTrackedNodes.ReplaceNode(trackedUsingStatement!, statements);
                }
                catch (Exception)
                {
                    treeWithTrackedNodes = treeWithTrackedNodes.ReplaceNode(trackedUsingStatement!.Parent!, statements);
                }
            }

            return treeWithTrackedNodes;
        }

        private IEnumerable<SyntaxNode> ReplaceExpressionStatements(SyntaxList<StatementSyntax> statements, int? index)
        {
            var nodesToBeReplaced = GetAllMoqExpressionStatements(statements).ToList();
            var parentTrivia = statements.First().Parent?.Parent?.GetLeadingTrivia();
            var parentTriviaWithoutNewLine = parentTrivia.ToString()!.Replace(Environment.NewLine, "");
            SyntaxTriviaList memberAccessExpressionTriviaWithoutNewLine;

            try
            {
                memberAccessExpressionTriviaWithoutNewLine = ((MemberAccessExpressionSyntax)nodesToBeReplaced.First()
                        .DescendantNodes()
                        .First(s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression)))
                    .Expression
                    .GetTrailingTrivia();
            }
            catch (InvalidOperationException)
            {
                memberAccessExpressionTriviaWithoutNewLine = SyntaxTriviaList.Empty;
            }

            for (var i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];
                if (!nodesToBeReplaced.Any(s => s.IsEquivalentTo(statement, false)))
                {
                    statements = statements.Replace(statement, statement.WithLeadingTrivia(parentTrivia));
                    continue;
                }

                var firstIdentifierName = statement.GetFirstIdentifierName();
                var newExpressionStatement = (ExpressionStatementSyntax)statement.ReplaceNode(
                    firstIdentifierName,
                    MoqSyntaxFactory.InSequenceExpression(
                        firstIdentifierName,
                        index,
                        memberAccessExpressionTriviaWithoutNewLine));

                statements = statements.Replace(
                    statement,
                    newExpressionStatement
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(parentTriviaWithoutNewLine))
                        .WithTrailingTrivia(SyntaxFactory.Whitespace(Environment.NewLine)));
            }

            statements = statements.Insert(
                0,
                MoqSyntaxFactory.MockSequenceLocalDeclarationStatement(index)
                    .WithLeadingTrivia(parentTrivia)
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(Environment.NewLine)));

            return statements;
        }

        private IEnumerable<ExpressionStatementSyntax> GetAllMoqExpressionStatements(SyntaxList<StatementSyntax> statements)
        {
            return statements
                .Where(s => s.IsKind(SyntaxKind.ExpressionStatement))
                .Select(s => (ExpressionStatementSyntax)s)
                .Where(
                    s => Model.GetSymbolInfo(s.Expression).Symbol?.OriginalDefinition is IMethodSymbol symbol
                         && MoqSymbols.AllMoqSetupSymbols.Contains(symbol, SymbolEqualityComparer.Default));
        }

        private IEnumerable<UsingStatementSyntax> GetRhinoMocksOrderedUsingStatements(MethodDeclarationSyntax node)
        {
            return node
                .GetOriginal(node, CompilationId)!
                .DescendantNodes()
                .Where(s => s.IsKind(SyntaxKind.UsingStatement))
                .Select(s => (UsingStatementSyntax)s)
                .Where(
                    s => s.Expression is InvocationExpressionSyntax invocationExpression
                         && RhinoMocksSymbols.OrderedSymbols.Contains(Model.GetSymbolInfo(invocationExpression).Symbol, SymbolEqualityComparer.Default));
        }
    }
}