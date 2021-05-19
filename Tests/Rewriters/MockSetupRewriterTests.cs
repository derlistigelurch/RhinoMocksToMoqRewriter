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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class MockSetupRewriterTests
  {
    private MockSetupRewriter _rewriter;
    private Mock<IFormatter> _formatter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            NamespaceContext =
                @"
static partial class PrivateInvoke
{
  public static object? InvokeNonPublicMethod (object target, string methodName, params object?[]? arguments)
  {
    return null;
  }
}",
            //language=C#
            InterfaceContext =
                @"
void DoSomething();
int DoSomething (int b);
T DoSomething<T> (Func<T> func);",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock;",
            //language=C#
            MethodContext = @"
var stub = MockRepository.GenerateStub<ITestInterface>();
var mock = MockRepository.GenerateMock<ITestInterface>();
var strictMock = MockRepository.GenerateStrictMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>())).Returns<SyntaxNode> (s => s);
      _rewriter = new MockSetupRewriter (_formatter.Object);
    }

    [Test]
    [TestCase (
        //language=C#
        @"stub.Stub (m => m.DoSomething());",
        //language=C#
        @"stub.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"mock.Stub (m => m.DoSomething());",
        //language=C#
        @"mock.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"
mock.Expect (m => m.DoSomething());",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
stub
  .Expect (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
stub
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
stub
  .Stub (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
stub
  .Setup (m => m.DoSomething (1))
  .Returns (2);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2)
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Stub (m => m.DoSomething (1))
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Callback (null);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2)
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething())
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Stub (m => m.DoSomething())
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething())
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Stub (mock, m => m.DoSomething());",
        //language=C#
        @"mock.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Expect (mock, m => m.DoSomething ());",
        //language=C#
        @"mock.Setup (m => m.DoSomething()).Verifiable();")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Expect (mock, m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Stub (mock, m => m.DoSomething())
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null);")]
    [TestCase (
        //language=C#
        @"_mock.Expect (_ => _.DoSomething (42)).Return (21).Throw (new InvalidOperationException (""mimimi""));",
        //language=C#
        @"_mock.Setup (_ => _.DoSomething (42)).Returns (21).Throws (new InvalidOperationException (""mimimi"")).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Stub (_ => _.DoSomething (42)).Return (21).Throw (new InvalidOperationException (""mimimi""));",
        //language=C#
        @"_mock.Setup (_ => _.DoSomething (42)).Returns (21).Throws (new InvalidOperationException (""mimimi""));")]
    [TestCase (
        //language=C#
        @"strictMock.Expect (m => m.DoSomething());",
        //language=C#
        @"strictMock.Setup (m => m.DoSomething()).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Any();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Do (null);",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, ""OnInit"", EventArgs.Empty));",
        //language=C#
        @"_mock.Protected().Setup (""OnInit"", true, EventArgs.Empty).Verifiable();")]
    public void Rewrite_MockSetup (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"
mock.DoSomething (
  () =>
  {
    mock
      .Expect (m => m.DoSomething (1)).Return (2);
    return mock;
  });",
        //language=C#
        @"
mock.DoSomething (
  () =>
  {
    mock
      .Setup (m => m.DoSomething (1)).Returns (2).Verifiable();
    return mock;
  });")]
    public void Rewrite_NestedMockSetup (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      var expectedExpressionStatements = expectedNode.DescendantNodes().Where (s => s.IsKind (SyntaxKind.ExpressionStatement)).ToList();
      var actualExpressionStatements = actualNode.DescendantNodes().Where (s => s.IsKind (SyntaxKind.ExpressionStatement)).ToList();

      Assert.AreEqual (expectedExpressionStatements.Count, actualExpressionStatements.Count);
      for (var i = 0; i < expectedExpressionStatements.Count; i++)
      {
        Assert.That (expectedExpressionStatements[i].IsEquivalentTo (actualExpressionStatements[i], false));
      }
    }

    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Once();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Twice();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.AtLeastOnce();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Never();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Times (1, 3);",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething()).Callback (null).Repeat.Times (4);",
        //language=C#
        @"_mock.Setup (m => m.DoSomething()).Callback (null).Verifiable();")]
    public void Rewrite_MockSetupWithVerifyAnnotation (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      var data = actualNode.GetAnnotations (MoqSyntaxFactory.VerifyAnnotationKind).Single().Data;
      Assert.NotNull (data);
      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}