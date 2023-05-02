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

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Wrapper
{
    public class SyntaxNodePair<T1, T2>
    {
        public T1 FirstNode { get; }

        public T2 SecondNode { get; }

        public bool IsValid => FirstNode is not null && SecondNode is not null;

        public SyntaxNodePair()
        {
            FirstNode = default!;
            SecondNode = default!;
        }

        public SyntaxNodePair(T1 firstNode, T2 secondNode)
        {
            FirstNode = firstNode;
            SecondNode = secondNode;
        }

        public static implicit operator T1(SyntaxNodePair<T1, T2> syntaxNodePair) => syntaxNodePair.FirstNode;
        public static implicit operator T2(SyntaxNodePair<T1, T2> syntaxNodePair) => syntaxNodePair.SecondNode;
        public static implicit operator SyntaxNodePair<T1, T2>((T1 firstNode, T2 secondNode) data) => new(data.firstNode, data.secondNode);
        public static implicit operator (T1, T2)(SyntaxNodePair<T1, T2> syntaxNodePair) => (syntaxNodePair.FirstNode, syntaxNodePair.SecondNode);
    }
}