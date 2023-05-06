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

using Microsoft.CodeAnalysis;

namespace RhinoMocksToMoqRewriter.Core.Wrapper
{
    public class SyntaxNodePair
    {
        public SyntaxNode? Original { get; } = null;

        public SyntaxNode? Base { get; } = null;

        public SyntaxNode? Tracked { get; } = null;

        public SyntaxNodePair(SyntaxNode? original, SyntaxNode? @base, SyntaxNode? tracked)
        {
            Original = original;
            Base = @base;
            Tracked = tracked;
        }

        public static implicit operator SyntaxNodePair((SyntaxNode? original, SyntaxNode? @base, SyntaxNode? tracked) nodes)
            => new(nodes.original, nodes.@base, nodes.tracked);
    }
}