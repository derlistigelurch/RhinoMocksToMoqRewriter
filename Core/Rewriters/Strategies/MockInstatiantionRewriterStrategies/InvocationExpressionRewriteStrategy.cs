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
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.MockInstatiantionRewriterStrategies
{
    public class InvocationExpressionRewriteStrategy : BaseMockInstantiationRewriteStrategy
    {
        public InvocationExpressionRewriteStrategy(Guid compilationId, SemanticModel model, MoqSymbols moq, RhinoMocksSymbols rhinoMocks)
            : base(compilationId, model, moq, rhinoMocks)
        {
        }

        public override bool TryRewrite(SyntaxNodePair nodes, out SyntaxNode? rewrittenNode)
        {
            rewrittenNode = null;
            var methodSymbol = (Model.GetSymbolInfo(nodes.Original!).Symbol as IMethodSymbol)?.OriginalDefinition;
            if (!ShouldConvert(methodSymbol))
            {
                return false;
            }

            var rhinoMocksMethodGenericName = nodes.Base?.GetFirstGenericNameOrDefault();
            (TypeArgumentListSyntax? moqMockTypeArgumentList, ArgumentListSyntax? moqMockArgumentSyntaxList) = rhinoMocksMethodGenericName == null
                ? GetDataFromMockWithoutGenericName((InvocationExpressionSyntax)nodes.Base!)
                : GetDataFromMockWithGenericName((InvocationExpressionSyntax)nodes.Base!, rhinoMocksMethodGenericName);

            if (!CanConvert(moqMockTypeArgumentList, moqMockArgumentSyntaxList))
            {
                return false;
            }

            rewrittenNode = Convert(methodSymbol!, nodes, moqMockTypeArgumentList!, moqMockArgumentSyntaxList!);
            return true;
        }

        private static bool ShouldConvert(IMethodSymbol? methodSymbol)
        {
            return methodSymbol != null;
        }

        private static bool CanConvert(TypeArgumentListSyntax? moqMockTypeArgumentList, ArgumentListSyntax? moqMockArgumentSyntaxList)
        {
            return !(moqMockTypeArgumentList == null && moqMockArgumentSyntaxList == null);
        }

        private SyntaxNode Convert(
            IMethodSymbol methodSymbol,
            SyntaxNodePair nodes,
            TypeArgumentListSyntax typeArgumentList,
            ArgumentListSyntax argumentList)
        {
            return methodSymbol switch
            {
                _ when RhinoMocksSymbols.AllGenerateMockAndStubSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => MoqSyntaxFactory.MockCreationExpression(typeArgumentList, argumentList),
                _ when RhinoMocksSymbols.AllPartialMockSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => MoqSyntaxFactory.PartialMockCreationExpression(typeArgumentList, argumentList),
                _ when RhinoMocksSymbols.AllStrictMockSymbols.Contains(methodSymbol, SymbolEqualityComparer.Default)
                    => MoqSyntaxFactory.StrictMockCreationExpression(typeArgumentList, argumentList),
                _ => nodes.Base!
            };
        }

        private static (TypeArgumentListSyntax, ArgumentListSyntax) GetDataFromMockWithGenericName(
            InvocationExpressionSyntax baseCallNode,
            GenericNameSyntax rhinoMocksMethodGenericName)
        {
            var moqMockTypeArgumentList = SyntaxFactory.TypeArgumentList().AddArguments(rhinoMocksMethodGenericName.TypeArgumentList.Arguments.First());
            var moqMockArgumentSyntaxList = baseCallNode.ArgumentList;

            return (moqMockTypeArgumentList, moqMockArgumentSyntaxList);
        }

        private static (TypeArgumentListSyntax?, ArgumentListSyntax?) GetDataFromMockWithoutGenericName(InvocationExpressionSyntax baseCallNode)
        {
            if (baseCallNode.ArgumentList.GetFirstArgumentOrDefault() is not { } typeArgument)
            {
                return (null, null);
            }

            var typeArgumentList = typeArgument.Expression switch
            {
                TypeOfExpressionSyntax { Type: { } type } => MoqSyntaxFactory.TypeArgumentList(type),
                TypeSyntax type => MoqSyntaxFactory.TypeArgumentList(type),
                _ => null
            };

            var argumentList = baseCallNode.ArgumentList.WithArguments(SyntaxFactory.SeparatedList(baseCallNode.ArgumentList.Arguments.Skip(1)));

            return (typeArgumentList, argumentList);
        }
    }
}