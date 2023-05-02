//  Copyright (c) rubicon IT GmbH
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class ExpectCallRewriter : RewriterBase
    {
        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var trackedNodes = node.TrackNodes(node.DescendantNodesAndSelf(), CompilationId);
            var baseCallNode = VisitBaseNodeAs<ExpressionStatementSyntax>(
                () => base.VisitExpressionStatement(trackedNodes));

            return RewriteExpectCall(baseCallNode).WithLeadingAndTrailingTriviaOfNode(node);
        }

        private SyntaxNode RewriteExpectCall(SyntaxNode node)
        {
            var symbol = Model.GetSymbolAs<ISymbol>(node.GetOriginalNode(node, CompilationId)!)?.OriginalDefinition;
            return node switch
            {
                ExpressionStatementSyntax expressionStatement 
                    => RewriteExpressionStatement(expressionStatement),
                MemberAccessExpressionSyntax memberAccessExpression when GetAllSimpleRhinoMocksSymbols().Contains(symbol, SymbolEqualityComparer.Default) 
                    => RewriteMemberAccessExpression(memberAccessExpression, node),
                InvocationExpressionSyntax {Expression: MemberAccessExpressionSyntax rhinoMocksMemberAccessExpression} rhinoMocksInvocationExpression 
                    => RewriteInvocationExpression(symbol, rhinoMocksInvocationExpression, rhinoMocksMemberAccessExpression, node),
                _ => node
            };
        }

        private SyntaxNode RewriteInvocationExpression(ISymbol? symbol, InvocationExpressionSyntax invocationExpression, MemberAccessExpressionSyntax memberAccessExpression, SyntaxNode node)
        {
            return symbol switch
            {
                _ when RhinoMocksSymbols.ExpectCallSymbols.Contains(symbol, SymbolEqualityComparer.Default)
                    => ConvertExpectExpression(invocationExpression).WithLeadingAndTrailingTriviaOfNode(node),
                _ when RhinoMocksSymbols.ConstraintsSymbols.Contains(symbol, SymbolEqualityComparer.Default)
                    => RewriteContainedExpression(memberAccessExpression, invocationExpression),
                _ when GetAllSimpleRhinoMocksSymbols().Contains(symbol, SymbolEqualityComparer.Default) 
                    => invocationExpression
                        .ReplaceNode(
                            node.GetCurrentNode(memberAccessExpression.Expression, CompilationId)!,
                            (ExpressionSyntax) RewriteExpectCall(memberAccessExpression.Expression)),
                _ => node
            };
        }
        
        private SyntaxNode RewriteExpressionStatement(ExpressionStatementSyntax expressionStatement)
        {
            return expressionStatement.WithExpression((ExpressionSyntax) RewriteExpectCall(expressionStatement.Expression));
        }

        private SyntaxNode RewriteMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression, SyntaxNode node)
        {
            return memberAccessExpression.ReplaceNode(
                node.GetCurrentNode(memberAccessExpression.Expression, CompilationId)!,
                (ExpressionSyntax) RewriteExpectCall(memberAccessExpression.Expression));
        }

        private SyntaxNode RewriteContainedExpression(MemberAccessExpressionSyntax memberAccessExpression, InvocationExpressionSyntax invocationExpression)
        {
            var containedExpression = RewriteExpectCall(memberAccessExpression.Expression);
            return ConvertMockedExpression(containedExpression, memberAccessExpression, invocationExpression);
        }
        
        private SyntaxNode ConvertMockedExpression(
            SyntaxNode rewrittenContainedExpression,
            MemberAccessExpressionSyntax originalRhinoMocksMemberAccessExpression,
            InvocationExpressionSyntax originalRhinoMocksInvocationExpression)
        {
            var rewrittenInvocationExpression =
                (InvocationExpressionSyntax) (rewrittenContainedExpression.GetFirstArgumentOrDefault()?.Expression as SimpleLambdaExpressionSyntax)?.ExpressionBody!;

            if (rewrittenInvocationExpression is null)
            {
                Console.Error.WriteLine("  WARNING: Unable to convert Expect.Call");
                return rewrittenContainedExpression;
            }

            var containedArgument = originalRhinoMocksMemberAccessExpression.Expression.GetFirstArgument();
            var originalContainedInvocationExpression = containedArgument.Expression switch
            {
                LambdaExpressionSyntax {Body: InvocationExpressionSyntax lambdaBodyInvocationExpression} => lambdaBodyInvocationExpression,
                InvocationExpressionSyntax invocationExpression => invocationExpression,
                _ => throw new InvalidOperationException("Unable to resolve inner argument.")
            };

            var containedInvocationExpressionParameterSymbols = GetMethodParameterTypes(originalContainedInvocationExpression).ToList();
            var rewrittenArgumentList = ConvertConstraints(originalRhinoMocksInvocationExpression.ArgumentList, containedInvocationExpressionParameterSymbols);

            return rewrittenContainedExpression.ReplaceNode(rewrittenInvocationExpression.ArgumentList, rewrittenArgumentList);
        }

        private static InvocationExpressionSyntax ConvertExpectExpression(InvocationExpressionSyntax rhinoMocksInvocationExpression)
        {
            var mockMethodCallExpression = rhinoMocksInvocationExpression.ArgumentList.Arguments.First().Expression;
            var firstIdentifierName = mockMethodCallExpression.GetFirstIdentifierName();

            var newExpression = mockMethodCallExpression.ReplaceNode(firstIdentifierName, MoqSyntaxFactory.LambdaParameterIdentifierName);
            if (newExpression is LambdaExpressionSyntax lambdaExpression)
            {
                newExpression = (InvocationExpressionSyntax) lambdaExpression.Body;
            }

            return MoqSyntaxFactory.InvocationExpression(
                MoqSyntaxFactory.MemberAccessExpression(firstIdentifierName, MoqSyntaxFactory.ExpectIdentifierName),
                MoqSyntaxFactory.SimpleArgumentList(MoqSyntaxFactory.SimpleLambdaExpression(newExpression!)));
        }

        private IEnumerable<ITypeSymbol> GetMethodParameterTypes(InvocationExpressionSyntax invocationExpression)
        {
            return ((IMethodSymbol) Model.GetSymbolInfo(invocationExpression.GetOriginalNode(invocationExpression, CompilationId)!).Symbol!).Parameters.Select(s => s.Type);
        }

        private ArgumentListSyntax ConvertConstraints(ArgumentListSyntax originalArgumentList, IReadOnlyList<ITypeSymbol> parameterTypes)
        {
            var convertedArguments = parameterTypes.Select((s, i) =>
                MoqSyntaxFactory.MatchesArgument(TypeSymbolToTypeSyntaxConverter.ConvertTypeSyntaxNodes(s, Generator), originalArgumentList.Arguments[i]));
            return MoqSyntaxFactory.SimpleArgumentList(convertedArguments);
        }

        private IEnumerable<ISymbol> GetAllSimpleRhinoMocksSymbols()
        {
            return RhinoMocksSymbols.AllIMethodOptionsSymbols
                .Concat(RhinoMocksSymbols.AllIRepeatSymbols)
                .Concat(RhinoMocksSymbols.IgnoreArgumentsSymbols);
        }
    }
}