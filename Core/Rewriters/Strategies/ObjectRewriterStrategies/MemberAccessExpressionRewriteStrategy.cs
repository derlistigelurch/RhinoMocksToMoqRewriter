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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public class MemberAccessExpressionRewriteStrategy : BaseObjectRewriteStrategy
    {
        public MemberAccessExpressionRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            if (ShouldConvert((MemberAccessExpressionSyntax)nodes.Base!))
            {
                return false;
            }

            if (!CanConvert(nodes.Base))
            {
                return false;
            }

            rewrittenNode = Convert(nodes.Original!, nodes.Base!);
            return true;
        }

        private SyntaxNode Convert(SyntaxNode node, SyntaxNode baseCallNode)
        {
            var currentNode = baseCallNode.GetCurrent(baseCallNode, CompilationId);
            var identifierNameToBeReplaced = currentNode!.GetFirstIdentifierName();
            try
            {
                return baseCallNode.ReplaceNode(
                    identifierNameToBeReplaced,
                    MoqSyntaxFactory.MockObjectExpression(identifierNameToBeReplaced)
                        .WithLeadingAndTrailingTriviaOfNode(identifierNameToBeReplaced)!);
            }
            catch (Exception)
            {
                EmitWarning("Unable to insert .Object", node);
                return baseCallNode;
            }
        }

        private bool CanConvert(SyntaxNode? node)
        {
            var currentNode = node?.GetCurrent(node, CompilationId);
            return currentNode is not null;
        }

        private bool ShouldConvert(MemberAccessExpressionSyntax node)
        {
            var nameSymbol = Model.GetSymbolInfo(node.GetOriginal(node, CompilationId)!.Name).GetFirstOverloadOrDefault();
            var typeSymbol = Model.GetTypeInfo(node.GetOriginal(node, CompilationId)!.Expression).Type?.OriginalDefinition;
            if (!MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }

            var extractedSymbol = (nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition;

            return
                MoqSymbols.AllMockSequenceSymbols
                    .Concat(RhinoMocksSymbols.AllBackToRecordSymbols)
                    .Concat(MoqSymbols.AllProtectedMockSymbols)
                    .Contains(extractedSymbol, SymbolEqualityComparer.Default);
        }
    }
}