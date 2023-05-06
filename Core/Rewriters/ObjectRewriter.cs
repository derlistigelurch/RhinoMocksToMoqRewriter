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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class ObjectRewriter : RewriterBase
    {
        private readonly Lazy<IRewriteStrategy> _memberAccessRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _argumentRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _returnStatementRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _initializerExpressionRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _assignmentExpressionRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _variableDeclaratorRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _objectCreationExpressionRewriteStrategy;
        private readonly Lazy<IRewriteStrategy> _parenthesizedLambdaExpressionRewriteStrategy;

        public ObjectRewriter()
        {
            _memberAccessRewriteStrategy = new(
                () => new MemberAccessExpressionRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _argumentRewriteStrategy = new(
                () => new ArgumentRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _returnStatementRewriteStrategy = new(
                () => new ReturnStatementRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _initializerExpressionRewriteStrategy = new(
                () => new InitializerExpressionRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _assignmentExpressionRewriteStrategy = new(
                () => new AssignmentExpressionRewriteRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _variableDeclaratorRewriteStrategy = new(
                () => new VariableDeclaratorRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _objectCreationExpressionRewriteStrategy = new(
                () => new ObjectCreationExpressionRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
            _parenthesizedLambdaExpressionRewriteStrategy = new(
                () => new ParenthesizedLambdaExpressionRewriteStrategy(CompilationId, Model, MoqSymbols, RhinoMocksSymbols));
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(trackedNodes)!;

            return _memberAccessRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (ArgumentSyntax)base.VisitArgument(trackedNodes)!;

            return _argumentRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (ReturnStatementSyntax)base.VisitReturnStatement(trackedNodes)!;

            return _returnStatementRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (InitializerExpressionSyntax)base.VisitInitializerExpression(trackedNodes)!;

            return _initializerExpressionRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (AssignmentExpressionSyntax)base.VisitAssignmentExpression(trackedNodes)!;

            return _assignmentExpressionRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var trackedNodes = TrackNodes(node);

            var baseCallNode = (VariableDeclaratorSyntax)base.VisitVariableDeclarator(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginal(baseCallNode, CompilationId)!;

            return _variableDeclaratorRewriteStrategy.Value.TryRewrite((originalNode, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(trackedNodes)!;

            return _objectCreationExpressionRewriteStrategy.Value.TryRewrite((node, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (ParenthesizedLambdaExpressionSyntax)base.VisitParenthesizedLambdaExpression(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginal(baseCallNode, CompilationId)!;

            return _parenthesizedLambdaExpressionRewriteStrategy.Value.TryRewrite((originalNode, baseCallNode, trackedNodes), out var rewrittenNode)
                ? rewrittenNode
                : baseCallNode;
        }

        private T TrackNodes<T>(T node)
            where T : SyntaxNode
        {
            return node.Track(
                node.DescendantNodesAndSelf()
                    .Where(
                        s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                             || s.IsKind(SyntaxKind.IdentifierName)
                             || s.IsKind(SyntaxKind.CollectionInitializerExpression)
                             || s.IsKind(SyntaxKind.ArrayInitializerExpression)
                             || s.IsKind(SyntaxKind.ObjectInitializerExpression)
                             || s.IsKind(SyntaxKind.ObjectCreationExpression)
                             || s.IsKind(SyntaxKind.VariableDeclarator)
                             || s.IsKind(SyntaxKind.ParenthesizedLambdaExpression)),
                CompilationId);
        }
    }
}