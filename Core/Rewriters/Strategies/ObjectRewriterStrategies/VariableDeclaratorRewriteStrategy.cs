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
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public class VariableDeclaratorRewriteStrategy : BaseObjectRewriteStrategy
    {
        public VariableDeclaratorRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            if (!ShouldConvert((VariableDeclaratorSyntax)nodes.Original!))
            {
                return false;
            }

            rewrittenNode = Convert((VariableDeclaratorSyntax)nodes.Base!);
            return true;
        }

        private static VariableDeclaratorSyntax Convert(VariableDeclaratorSyntax baseCallNode)
        {
            return baseCallNode
                .WithInitializer(
                    baseCallNode.Initializer!
                        .WithValue(MoqSyntaxFactory.MockObjectExpression(baseCallNode.Initializer?.Value!))
                        .WithLeadingAndTrailingTriviaOfNode(baseCallNode.Initializer?.Value!))
                .WithLeadingAndTrailingTriviaOfNode(baseCallNode.Initializer!);
        }

        private bool ShouldConvert(VariableDeclaratorSyntax node)
        {
            var initializerValue = node.Initializer?.Value;
            if (initializerValue is null or not (IdentifierNameSyntax or ObjectCreationExpressionSyntax))
            {
                return false;
            }

            var identifierType = Model.GetTypeInfo(((VariableDeclarationSyntax)(node.Parent!)).Type).Type?.BaseType;
            if (MoqSymbols.MoqSymbol.Equals(identifierType, SymbolEqualityComparer.Default))
            {
                return false;
            }

            var initializerType = Model.GetTypeInfo(node.Initializer!.Value).Type?.BaseType;
            return MoqSymbols.MoqSymbol.Equals(initializerType, SymbolEqualityComparer.Default);
        }
    }
}