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
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.MockInstatiantionRewriterStrategies
{
    public class LocalDeclarationStatementRewriteStrategy : BaseMockInstantiationRewriteStrategy
    {
        public LocalDeclarationStatementRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            if (!ShouldConvert((LocalDeclarationStatementSyntax)nodes.Base!))
            {
                return false;
            }

            rewrittenNode = Convert((LocalDeclarationStatementSyntax)nodes.Base!);
            return true;
        }

        private static SyntaxNode Convert(LocalDeclarationStatementSyntax node)
        {
            return node.WithDeclaration(
                node.Declaration
                    .WithType(
                        MoqSyntaxFactory.VarType
                            .WithLeadingTrivia(node.Declaration.Type.GetLeadingTrivia())
                            .WithTrailingTrivia(node.Declaration.Type.GetTrailingTrivia())));
        }

        private bool ShouldConvert(LocalDeclarationStatementSyntax node)
        {
            return node.GetOriginal(node, CompilationId)!.Declaration.Variables
                       .Any(
                           s => s.Initializer is { } initializer
                                && Model.GetSymbolInfo(initializer.Value).Symbol?.ContainingType is { } symbol
                                && RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals(symbol, SymbolEqualityComparer.Default))
                   && !node.Declaration.Type.IsEquivalentTo(MoqSyntaxFactory.VarType, false);
        }
    }
}