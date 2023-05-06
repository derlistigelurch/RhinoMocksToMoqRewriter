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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public class AssignmentExpressionRewriteRewriteStrategy : BaseObjectRewriteStrategy
    {
        public AssignmentExpressionRewriteRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            try
            {
                if (!ShouldConvert(nodes))
                {
                    return false;
                }

                rewrittenNode = Convert((AssignmentExpressionSyntax)nodes.Base!);
                return true;
            }
            catch (Exception)
            {
                EmitWarning("Unable to insert .Object", nodes.Original!);
                return false;
            }
        }

        private bool ShouldConvert(SyntaxNodePair nodes)
        {
            return CanConvertRightNode((AssignmentExpressionSyntax)nodes.Base!)
                   && CanConvertLeftNode((AssignmentExpressionSyntax)nodes.Base!);
        }

        private static SyntaxNode Convert(AssignmentExpressionSyntax node)
        {
            return node.WithRight(MoqSyntaxFactory.MockObjectExpression(node.Right));
        }

        private bool CanConvertLeftNode(AssignmentExpressionSyntax node)
        {
            var leftType = Model.GetTypeInfo(
                    node.GetOriginal(
                        node.Left, CompilationId)!)
                .Type?
                .BaseType;

            return !MoqSymbols.MoqSymbol.Equals(leftType, SymbolEqualityComparer.Default);
        }

        private bool CanConvertRightNode(AssignmentExpressionSyntax node)
        {
            if (node.Right is not IdentifierNameSyntax and not ObjectCreationExpressionSyntax)
            {
                return false;
            }

            var rightType = Model.GetTypeInfo(
                    node.GetOriginal(
                        node.Right, CompilationId)!)
                .Type?
                .BaseType;

            return MoqSymbols.MoqSymbol.Equals(rightType, SymbolEqualityComparer.Default);
        }
    }
}