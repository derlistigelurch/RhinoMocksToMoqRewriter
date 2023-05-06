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
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public class ObjectCreationExpressionRewriteStrategy : BaseObjectRewriteStrategy
    {
        public ObjectCreationExpressionRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            if (!ShouldConvertObjectCreationExpression(nodes.Base!))
            {
                return false;
            } 
            
            rewrittenNode = ConvertVisitObjectCreationExpression((ExpressionSyntax)nodes.Base!, nodes.Original!);
            return true;
        }

        private static SyntaxNode? ConvertVisitObjectCreationExpression(ExpressionSyntax baseCallNode, SyntaxNode originalNode)
        {
            return MoqSyntaxFactory.MockObjectExpression(baseCallNode).WithLeadingAndTrailingTriviaOfNode(originalNode);
        }

        private bool ShouldConvertObjectCreationExpression(SyntaxNode baseCallNode)
        {
            var originalNode = baseCallNode.GetOriginal(baseCallNode, CompilationId)!;
            if (!Model.TryGetSymbolAs<ISymbol>(originalNode, out var symbol))
            {
                return false;
            }

            symbol = symbol!.OriginalDefinition.ContainingSymbol;
            if (!MoqSymbols.GenericMoqSymbol.Equals(symbol, SymbolEqualityComparer.Default))
            {
                return false;
            }

            return !IsAncestorOfAny(
                originalNode,
                new[]
                {
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKind.LocalDeclarationStatement,
                    SyntaxKind.FieldDeclaration
                });
        }

        private static bool IsAncestorOfAny(SyntaxNode originalNode, IEnumerable<SyntaxKind> kinds)
        {
            return originalNode.Ancestors().Any(s => kinds.Any(s.IsKind));
        }
    }
}