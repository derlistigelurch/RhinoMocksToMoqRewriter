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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.MockInstatiantionRewriterStrategies;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class MockInstantiationRewriter : RewriterBase
    {
        private readonly IFormatter _formatter;
        private readonly Lazy<IRewriteStrategy> _localDeclarationRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _invocationExpressionRewriteStrategy;

        public MockInstantiationRewriter(IFormatter formatter)
        {
            _formatter = formatter;

            _localDeclarationRewriteStrategy = new(
                () => new LocalDeclarationStatementRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _invocationExpressionRewriteStrategy = new(
                () => new InvocationExpressionRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
        }

        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var trackedNodes = node.Track(
                node.DescendantNodesAndSelf().Where(s => s.IsKind(SyntaxKind.LocalDeclarationStatement) || s.IsKind(SyntaxKind.InvocationExpression)),
                CompilationId);
            var baseCallNode = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(trackedNodes)!;

            return _localDeclarationRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var trackedNodes = node.Track(
                node.DescendantNodesAndSelf().Where(s => s.IsKind(SyntaxKind.LocalDeclarationStatement) || s.IsKind(SyntaxKind.InvocationExpression)),
                CompilationId);
            var baseCallNode = (InvocationExpressionSyntax)base.VisitInvocationExpression(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginal(baseCallNode, CompilationId)!;

            return _invocationExpressionRewriteStrategy.Value.TryRewrite((originalNode, baseCallNode, trackedNodes), out var rewrittenNode)
                ? _formatter.Format(rewrittenNode!).WithLeadingAndTrailingTriviaOfNode(baseCallNode)
                : baseCallNode;
        }
    }
}