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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ObjectRewriterStrategies
{
    public class InitializerExpressionRewriteStrategy : BaseObjectRewriteStrategy
    {
        public InitializerExpressionRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            try
            {
                var newExpressions = CreateNewExpressions(nodes);
                rewrittenNode = ((InitializerExpressionSyntax)nodes.Base!).WithExpressions(newExpressions);
                return true;
            }
            catch
            {
                EmitWarning("Unable to insert .Object", nodes.Original!);
                return false;
            }
        }

        private SeparatedSyntaxList<ExpressionSyntax> CreateNewExpressions(SyntaxNodePair nodes)
        {
            var expressions = ((InitializerExpressionSyntax)nodes.Base!).Expressions;
            var updateAbleExpressions = GetUpdateableExpressions((InitializerExpressionSyntax)nodes.Base!).ToList();

            for (SyntaxNodePosition i = 0; i < updateAbleExpressions.Count; i++)
            {
                var expression = expressions[i];
                expressions = UpdateExpressions((InitializerExpressionSyntax)nodes.Base!, expressions, i, expression);
            }

            return expressions;
        }

        private IEnumerable<ExpressionSyntax> GetUpdateableExpressions(InitializerExpressionSyntax baseCallNode)
        {
            return baseCallNode.Expressions.Where(s => CanUpdateExpression(baseCallNode, s));
        }

        private static SeparatedSyntaxList<ExpressionSyntax> UpdateExpressions(
            InitializerExpressionSyntax baseCallNode,
            SeparatedSyntaxList<ExpressionSyntax> expressions,
            SyntaxNodePosition position,
            ExpressionSyntax expression)
        {
            if (position == baseCallNode.Expressions.Count - 1)
            {
                return expressions.Replace(
                    expression,
                    MoqSyntaxFactory.MockObjectExpression(expression.WithoutTrailingTrivia())
                        .WithTrailingTrivia(expression.GetTrailingTrivia()));
            }

            return expressions.Replace(expression, MoqSyntaxFactory.MockObjectExpression(expression));
        }


        private bool CanUpdateExpression(SyntaxNode baseCallNode, ExpressionSyntax expression)
        {
            if (expression is not IdentifierNameSyntax and not ObjectCreationExpressionSyntax)
            {
                return false;
            }

            var typeSymbol = Model.GetTypeInfo(baseCallNode.GetOriginal(expression, CompilationId)!).Type?.OriginalDefinition;
            return MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default);
        }
    }
}