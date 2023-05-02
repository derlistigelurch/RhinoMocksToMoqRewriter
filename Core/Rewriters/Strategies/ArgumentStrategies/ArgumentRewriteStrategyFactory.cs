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

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies
{
    public static class ArgumentRewriteStrategyFactory
    {
        public static IArgumentRewriteStrategy GetRewriteStrategy(ArgumentSyntax node, SemanticModel model, RhinoMocksSymbols rhinoMocksSymbols)
        {
            var symbol = model.GetSymbolInfo(node.Expression).Symbol?.OriginalDefinition;
            if (symbol is null)
            {
                return new DefaultArgumentRewriteStrategy();
            }

            return symbol switch
            {
                _ when rhinoMocksSymbols.ArgIsSymbols.Contains(symbol) => ArgIsArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgListEqualSymbols.Contains(symbol) => ArgListEqualArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgListIsInSymbols.Contains(symbol) => ArgListIsInArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgListContainsAll.Contains(symbol) => ArgListContainsAllArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgMatchesSymbols.Contains(symbol) => ArgMatchesArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsAnythingSymbols.Contains(symbol) => ArgIsAnythingArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsEqualSymbols.Contains(symbol) => ArgIsArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsNotEqualSymbols.Contains(symbol) => ArgIsNotEqualArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsSameSymbols.Contains(symbol) => ArgIsArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsNotSameSymbols.Contains(symbol) => ArgIsNotSameArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsNullSymbols.Contains(symbol) => ArgIsNullArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsNotNullSymbols.Contains(symbol) => ArgIsNotNullArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsGreaterThanSymbols.Contains(symbol) => ArgIsGreaterThanArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsGreaterThanOrEqualSymbols.Contains(symbol) => ArgIsGreaterThanOrEqualArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsLessThanSymbols.Contains(symbol) => ArgIsLessThanArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgIsLessThaOrEqualSymbols.Contains(symbol) => ArgIsLessThanOrEqualArgumentRewriteStrategy.Instance,
                _ when rhinoMocksSymbols.ArgTextLikeSymbols.Contains(symbol) => ArgIsArgumentRewriteStrategy.Instance,
                _ => DefaultArgumentRewriteStrategy.Instance
            };
        }
    }
}