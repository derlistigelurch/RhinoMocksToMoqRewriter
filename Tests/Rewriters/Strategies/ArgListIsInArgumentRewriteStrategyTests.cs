﻿//  Copyright (c) rubicon IT GmbH
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
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies
{
  [TestFixture]
  public class ArgListIsInArgumentRewriteStrategyTests
  {
    private readonly IArgumentRewriteStrategy _strategy = new ArgListIsInArgumentArgumentRewriteStrategy();

    [Test]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.IsIn (2));", "mock.DoSomething (It.Is<int[]> (param => param.Contains (2));")]
    public void Rewrite_ArgListIsIn (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgument (source);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileArgument (expected);
      var actualNode = _strategy.Rewrite (node);

      Assert.That (expectedArgumentNode.IsEquivalentTo (actualNode, false));
    }
  }
}