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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Wrapper;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
    public class ObjectRewriter : RewriterBase
    {
        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression(trackedNodes)!;

            var nameSymbol = Model.GetSymbolInfo(baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!.Name).GetFirstOverloadOrDefault();
            var typeSymbol = Model.GetTypeInfo(baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!.Expression).Type?.OriginalDefinition;
            if (!MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            if (MoqSymbols.AllProtectedMockSymbols.Contains((nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            if (MoqSymbols.AllMockSequenceSymbols.Contains((nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            if (RhinoMocksSymbols.AllBackToRecordSymbols.Contains((nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            var currentNode = baseCallNode.GetCurrentNode(baseCallNode, CompilationId);
            if (currentNode is null)
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to insert .Object"
                    + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

                return baseCallNode;
            }

            var identifierNameToBeReplaced = currentNode!.GetFirstIdentifierName();
            try
            {
                return baseCallNode.ReplaceNode(
                    identifierNameToBeReplaced,
                    MoqSyntaxFactory.MockObjectExpression(identifierNameToBeReplaced)
                        .WithLeadingAndTrailingTriviaOfNode(identifierNameToBeReplaced)!);
            }
            catch (Exception)
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to insert .Object"
                    + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

                return baseCallNode;
            }
        }

        public override SyntaxNode? VisitArgument(ArgumentSyntax node)
        {
            var trackedNodes = TrackNodes(node);

            var baseCallNode = (ArgumentSyntax) base.VisitArgument(trackedNodes)!;
            if (baseCallNode.Expression is not IdentifierNameSyntax identifierName)
            {
                return baseCallNode;
            }

            var typeSymbol = Model.GetTypeInfo(baseCallNode.GetOriginalNode(identifierName, CompilationId)!).Type?.OriginalDefinition;
            if (!MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            return baseCallNode.WithExpression(MoqSyntaxFactory.MockObjectExpression(identifierName));
        }

        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (ReturnStatementSyntax) base.VisitReturnStatement(trackedNodes)!;
            if (baseCallNode.Expression is not IdentifierNameSyntax identifierName)
            {
                return baseCallNode;
            }

            var typeSymbol = Model.GetTypeInfo(baseCallNode.GetOriginalNode(identifierName, CompilationId)!).Type?.OriginalDefinition;
            if (!MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            return baseCallNode.WithExpression(MoqSyntaxFactory.MockObjectExpression(identifierName));
        }

        public override SyntaxNode? VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (InitializerExpressionSyntax) base.VisitInitializerExpression(trackedNodes)!;

            try
            {
                var newExpressions = CreateNewExpressions(node, baseCallNode, baseCallNode.Expressions);
                return baseCallNode.WithExpressions(newExpressions);
            }
            catch
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to insert .Object"
                    + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

                return node;
            }
        }

        private SeparatedSyntaxList<ExpressionSyntax> CreateNewExpressions(
            InitializerExpressionSyntax node,
            InitializerExpressionSyntax baseCallNode,
            SeparatedSyntaxList<ExpressionSyntax> expressions)
        {
            var updateAbleExpressions = GetUpdateableExpressions(baseCallNode).ToList();

            for (SyntaxNodePosition i = 0; i < updateAbleExpressions.Count; i++)
            {
                var expression = expressions[i];
                expressions = UpdateExpressions(baseCallNode, expressions, i, expression);
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
            SyntaxNodePosition position, // int i
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

            var typeSymbol = Model.GetTypeInfo(baseCallNode.GetOriginalNode(expression, CompilationId)!).Type?.OriginalDefinition;
            return MoqSymbols.GenericMoqSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default);
        }

        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);
            var baseCallNode = (AssignmentExpressionSyntax) base.VisitAssignmentExpression(trackedNodes)!;

            if (baseCallNode.Right is not IdentifierNameSyntax and not ObjectCreationExpressionSyntax)
            {
                return baseCallNode;
            }

            INamedTypeSymbol? rightType;
            try
            {
                rightType = Model.GetTypeInfo(baseCallNode.GetOriginalNode(baseCallNode.Right, CompilationId)!).Type?.BaseType;
            }
            catch (Exception)
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to insert .Object"
                    + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

                return baseCallNode;
            }

            if (!MoqSymbols.MoqSymbol.Equals(rightType, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            INamedTypeSymbol? leftType;
            try
            {
                leftType = Model.GetTypeInfo(baseCallNode.GetOriginalNode(baseCallNode.Left, CompilationId)!).Type?.BaseType;
            }
            catch (Exception)
            {
                Console.Error.WriteLine(
                    $"  WARNING: Unable to insert .Object"
                    + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

                return baseCallNode;
            }

            if (MoqSymbols.MoqSymbol.Equals(leftType, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            return baseCallNode.WithRight(MoqSyntaxFactory.MockObjectExpression(baseCallNode.Right));
        }

        public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var trackedNodes = TrackNodes(node);

            var baseCallNode = (VariableDeclaratorSyntax) base.VisitVariableDeclarator(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!;

            var initializerValue = baseCallNode.Initializer?.Value;
            if (initializerValue is null or not IdentifierNameSyntax and not ObjectCreationExpressionSyntax)
            {
                return baseCallNode;
            }

            var identifierType = Model.GetTypeInfo(((VariableDeclarationSyntax) (originalNode.Parent!)).Type).Type?.BaseType;
            if (MoqSymbols.MoqSymbol.Equals(identifierType, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            var initializerType = Model.GetTypeInfo(originalNode.Initializer!.Value).Type?.BaseType;
            if (!MoqSymbols.MoqSymbol.Equals(initializerType, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            return baseCallNode
                .WithInitializer(
                    baseCallNode.Initializer!
                        .WithValue(MoqSyntaxFactory.MockObjectExpression(initializerValue!))
                        .WithLeadingAndTrailingTriviaOfNode(initializerValue!))
                .WithLeadingAndTrailingTriviaOfNode(baseCallNode.Initializer);
        }

        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);

            var baseCallNode = (ObjectCreationExpressionSyntax) base.VisitObjectCreationExpression(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!;

            var symbol = Model.GetSymbolInfo(originalNode).Symbol?.OriginalDefinition.ContainingSymbol;
            if (!MoqSymbols.GenericMoqSymbol.Equals(symbol, SymbolEqualityComparer.Default))
            {
                return baseCallNode.WithLeadingAndTrailingTriviaOfNode(node);
            }

            if (originalNode.Parent is ArgumentSyntax)
            {
                return MoqSyntaxFactory.MockObjectExpression(baseCallNode).WithLeadingAndTrailingTriviaOfNode(node);
            }

            if (originalNode.Ancestors().Any(s => s.IsKind(SyntaxKind.SimpleAssignmentExpression))
                || originalNode.Ancestors().Any(s => s.IsKind(SyntaxKind.LocalDeclarationStatement))
                || originalNode.Ancestors().Any(s => s.IsKind(SyntaxKind.FieldDeclaration)))
            {
                return baseCallNode.WithLeadingAndTrailingTriviaOfNode(node);
            }

            return MoqSyntaxFactory.MockObjectExpression(baseCallNode).WithLeadingAndTrailingTriviaOfNode(node);
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var trackedNodes = TrackNodes(node);

            var baseCallNode = (ParenthesizedLambdaExpressionSyntax) base.VisitParenthesizedLambdaExpression(trackedNodes)!;
            var originalNode = baseCallNode.GetOriginalNode(baseCallNode, CompilationId)!;
            if (originalNode.ExpressionBody is not IdentifierNameSyntax identifierName)
            {
                return baseCallNode;
            }

            var type = Model.GetTypeInfo(identifierName).Type?.BaseType;
            if (!MoqSymbols.MoqSymbol.Equals(type, SymbolEqualityComparer.Default))
            {
                return baseCallNode;
            }

            return baseCallNode.WithExpressionBody(
                MoqSyntaxFactory.MockObjectExpression(identifierName)
                    .WithLeadingAndTrailingTriviaOfNode(identifierName));
        }

        private T TrackNodes<T>(T node)
            where T : SyntaxNode
        {
            return node.TrackNodes(
                node.DescendantNodesAndSelf()
                    .Where(
                        s => s.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                             || s.IsKind(SyntaxKind.IdentifierName)
                             || s.IsKind(SyntaxKind.CollectionInitializerExpression)
                             || s.IsKind(SyntaxKind.ArrayInitializerExpression)
                             || s.IsKind(SyntaxKind.ObjectInitializerExpression)
                             || s.IsKind(SyntaxKind.ObjectCreationExpression)
                             || s.IsKind(SyntaxKind.VariableDeclarator)
                             || s.IsKind(SyntaxKind.ParenthesizedLambdaExpression)),
                CompilationId);
        }
    }
}