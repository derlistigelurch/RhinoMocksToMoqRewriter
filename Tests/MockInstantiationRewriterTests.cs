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
using RhinoMocksToMoqRewriter.Core;

namespace RhinoMocksToMoqRewriter.Tests
{
  [TestFixture]
  public class MockInstantiationRewriterTests
  {
    [Test]
    [TestCase ("_mock = MockRepository.GenerateMock<string>();", "_mock = new Mock<string>();")]
    [TestCase ("_mock = MockRepository.GenerateMock<string, IDisposable>();", "_mock = new Mock<string>();")]
    [TestCase ("_mock = MockRepository.GenerateMock<string, IDisposable, IConvertible>();", "_mock = new Mock<string>();")]
    [TestCase ("_mock = MockRepository.GeneratePartialMock<string> (42);", "_mock = new Mock<string> (42);")]
    [TestCase ("_mock = MockRepository.GeneratePartialMock<string>();", "_mock = new Mock<string>();")]
    [TestCase ("_mock = MockRepository.GenerateStrictMock<string>();", "_mock = new Mock<string> (MockBehaviour.Strict);")]
    [TestCase ("_mock = MockRepository.GenerateStrictMock<string> (42);", "_mock = new Mock<string> (MockBehaviour.Strict, 42);")]
    [TestCase ("_mock = MockRepository.GenerateStrictMock<string, IDisposable>();", "_mock = new Mock<string> (MockBehaviour.Strict);")]
    [TestCase ("_mock = MockRepository.GenerateStub<string>();", "_mock = new Mock<string>();")]
    [TestCase ("_symbol = GetSymbolInfo();", "_symbol = GetSymbolInfo();")]
    [TestCase ("_mock = MockRepository.GenerateMock<string>();", "_mock = new Mock<string>();")]
    [TestCase ("_mock = MockRepository.GenerateMock<string> (42, 32);", "_mock = new Mock<string> (42, 32);")]
    [TestCase (
        @"      _mock = MockRepository.GenerateMock<string> (
          42,
          32,
          43);",
        @"_mock = new Mock<string> (
          42,
          32,
          43);")]
    [TestCase (
        @"      _mock = MockRepository.GenerateStrictMock<string> (
          42,
          32,
          43);",
        @"_mock = new Mock<string> (
          MockBehaviour.Strict,
          42,
          32,
          43);")]
    [TestCase (
        @"_mock = MockRepository.GenerateStrictMock<string> (
    42,
    32,
    43);",
        @"_mock = new Mock<string> (
    MockBehaviour.Strict,
    42,
    32,
    43);")]
    [TestCase (
        @"_mock = MockRepository.GenerateMock<string> (MockRepository.GenerateStrictMock<string>());",
        @"_mock = new Mock<string> (new Mock<string> (MockBehaviour.Strict));")]
    public void Rewrite_ExpressionStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatement (source);
      var rewriter = new MockInstantiationRewriter (model);
      var newNode = rewriter.VisitExpressionStatement (node);
      Assert.AreEqual (expected, newNode!.ToString());
    }

    [Test]
    [TestCase ("var mock = MockRepository.GenerateStub<string>();", "var mock = new Mock<string>();")]
    [TestCase (
        @"      var mock = MockRepository.GenerateStrictMock<string> (
          42,
          32,
          43);",
        @"var mock = new Mock<string> (
          MockBehaviour.Strict,
          42,
          32,
          43);")]
    [TestCase (
        @"  var mock = MockRepository.GenerateStrictMock<string> (
      42,
      32,
      43);",
        @"var mock = new Mock<string> (
      MockBehaviour.Strict,
      42,
      32,
      43);")]
    public void Rewrite_LocalDeclarationStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileLocalDeclarationStatement (source);
      var rewriter = new MockInstantiationRewriter (model);
      var newNode = rewriter.VisitLocalDeclarationStatement (node);
      Assert.AreEqual (expected, newNode!.ToString());
    }

    [Test]
    [TestCase ("private Mock<string> _mock = MockRepository.GenerateMock<string>();", "private Mock<string> _mock = new Mock<string>();")]
    [TestCase (
        @"    public Mock<string> Mock = MockRepository.GenerateMock<string> (
        42,
        32,
        43);",
        @"public Mock<string> Mock = new Mock<string> (
        42,
        32,
        43);")]
    [TestCase (
        @"        public Mock<string> Mock = MockRepository.GenerateMock<string> (
            42,
            32,
            43);",
        @"public Mock<string> Mock = new Mock<string> (
            42,
            32,
            43);")]
    [TestCase (
        @"                public Mock<string> Mock = MockRepository.GenerateMock<string> (
                    42,
                    32,
                    43);",
        @"public Mock<string> Mock = new Mock<string> (
                    42,
                    32,
                    43);")]
    public void Rewrite_FieldDeclarationStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileFieldDeclaration (source);
      var rewriter = new MockInstantiationRewriter (model);
      var newNode = rewriter.VisitFieldDeclaration (node);
      Assert.AreEqual (expected, newNode!.ToString());
    }
  }
}