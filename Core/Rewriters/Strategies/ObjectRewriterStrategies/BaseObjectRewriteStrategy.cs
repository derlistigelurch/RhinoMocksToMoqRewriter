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
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public abstract class BaseObjectRewriteStrategy : IRewriteStrategy
    {
        protected Guid CompilationId { get; }

        protected SemanticModel Model { get; }
        
        protected RhinoMocksSymbols RhinoMocksSymbols { get; }

        protected MoqSymbols MoqSymbols { get; }

        protected BaseObjectRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
        {
            CompilationId = compilationId;
            Model = model;
            RhinoMocksSymbols = rhinoMocks;
            MoqSymbols = moq;
        }

        public abstract bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode);

        public virtual TNode TrackNodes<TNode>(SyntaxNode node) where TNode : SyntaxNode
        {
            return (TNode)node.Track(
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
        
        protected static void EmitWarning(string warning, SyntaxNode node)
        {
            Console.Error.WriteLine(
                $"  WARNING: {warning}"
                + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");
        }
    }
}