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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Wrapper
{
    public class SyntaxNodeWithIndex<T> where T : SyntaxNode
    {
        public int Index { get; }
        
        public T Node { get; }

        public SyntaxNodeWithIndex(T node, int index)
        {
            Node = node;
            Index = index;
        }
        
        public static implicit operator SyntaxNodeWithIndex<T>((T node, int index) data) => new(data.node, data.index);
        public static implicit operator int(SyntaxNodeWithIndex<T> nodeWithIndex) => nodeWithIndex.Index;
        public static implicit operator T(SyntaxNodeWithIndex<T> nodeWithIndex) => nodeWithIndex.Node;
    }
}