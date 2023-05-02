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

namespace RhinoMocksToMoqRewriter.Core
{
    public class RhinoMocksSymbols
    {
        public RhinoMocksSymbols(Compilation compilation)
        {
            RhinoMocksMockRepositorySymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.MockRepository")!;
            RhinoMocksArgSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Arg")!;
            RhinoMocksConstraintsListArgSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.ListArg`1")!;
            RhinoMocksGenericArgSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Arg`1")!;
            RhinoMocksConstraintsIsArgSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.IsArg`1")!;
            RhinoMocksArgTextSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.TextArg")!;
            RhinoMocksConstraintIsSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.Is")!;
            RhinoMocksConstraintListSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.List")!;
            RhinoMocksConstraintPropertySymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Constraints.Property")!;
            RhinoMocksExtensionSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.RhinoMocksExtensions")!;
            RhinoMocksIRepeatSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Interfaces.IRepeat`1")!;
            RhinoMocksIMethodOptionsSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Interfaces.IMethodOptions`1")!;
            RhinoMocksExpectSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.Expect")!;
            RhinoMocksLastCallSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.LastCall")!;
            RhinoMocksSetupResultSymbol = compilation.GetTypeByMetadataName("Rhino.Mocks.SetupResult")!;

            if (AllTypeSymbols.Any(s => s is null))
            {
                throw new InvalidOperationException("Unable to get Rhino.Mocks Symbols.\r\n"
                                                    + "Make sure the correct version of Rhino.Mocks is installed in:" + compilation.AssemblyName);
            }
        }

        private IReadOnlyList<INamedTypeSymbol>? _allTypeSymbols;

        public IReadOnlyList<INamedTypeSymbol?> AllTypeSymbols
        {
            get
            {
                if (_allTypeSymbols is null)
                {
                    _allTypeSymbols = new List<INamedTypeSymbol>
                    {
                        RhinoMocksMockRepositorySymbol,
                        RhinoMocksArgSymbol,
                        RhinoMocksConstraintsListArgSymbol,
                        RhinoMocksGenericArgSymbol,
                        RhinoMocksConstraintsIsArgSymbol,
                        RhinoMocksArgTextSymbol,
                        RhinoMocksConstraintIsSymbol,
                        RhinoMocksConstraintListSymbol,
                        RhinoMocksConstraintPropertySymbol,
                        RhinoMocksExtensionSymbol,
                        RhinoMocksIRepeatSymbol,
                        RhinoMocksIMethodOptionsSymbol,
                        RhinoMocksExpectSymbol,
                        RhinoMocksLastCallSymbol,
                        RhinoMocksSetupResultSymbol
                    };
                }

                return _allTypeSymbols;
            }
        }

        #region TypeSymbols

        public INamedTypeSymbol RhinoMocksMockRepositorySymbol { get; }
        public INamedTypeSymbol RhinoMocksArgSymbol { get; }
        public INamedTypeSymbol RhinoMocksConstraintsListArgSymbol { get; }
        public INamedTypeSymbol RhinoMocksGenericArgSymbol { get; }
        public INamedTypeSymbol RhinoMocksConstraintsIsArgSymbol { get; }
        public INamedTypeSymbol RhinoMocksArgTextSymbol { get; }
        public INamedTypeSymbol RhinoMocksConstraintIsSymbol { get; }
        public INamedTypeSymbol RhinoMocksConstraintListSymbol { get; }
        public INamedTypeSymbol RhinoMocksConstraintPropertySymbol { get; }
        public INamedTypeSymbol RhinoMocksExtensionSymbol { get; }
        public INamedTypeSymbol RhinoMocksIRepeatSymbol { get; }
        public INamedTypeSymbol RhinoMocksIMethodOptionsSymbol { get; }
        public INamedTypeSymbol RhinoMocksExpectSymbol { get; }
        public INamedTypeSymbol RhinoMocksLastCallSymbol { get; }
        public INamedTypeSymbol RhinoMocksSetupResultSymbol { get; }

        #endregion

        #region MockRepository

        private IReadOnlyList<ISymbol>? _mockRepositoryGenerateMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryGenerateMockSymbols
        {
            get
            {
                if (_mockRepositoryGenerateMockSymbols is null)
                {
                    _mockRepositoryGenerateMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("GenerateMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryGenerateMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryGenerateStubSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryGenerateStubSymbols
        {
            get
            {
                if (_mockRepositoryGenerateStubSymbols is null)
                {
                    _mockRepositoryGenerateStubSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("GenerateStub")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryGenerateStubSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryDynamicMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryDynamicMockSymbols
        {
            get
            {
                if (_mockRepositoryDynamicMockSymbols is null)
                {
                    _mockRepositoryDynamicMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("DynamicMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryDynamicMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryDynamicMultiMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryDynamicMultiMockSymbols
        {
            get
            {
                if (_mockRepositoryDynamicMultiMockSymbols is null)
                {
                    _mockRepositoryDynamicMultiMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("DynamicMultiMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryDynamicMultiMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryStubSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryStubSymbols
        {
            get
            {
                if (_mockRepositoryStubSymbols is null)
                {
                    _mockRepositoryStubSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("Stub")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryStubSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryPartialMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryPartialMockSymbols
        {
            get
            {
                if (_mockRepositoryPartialMockSymbols is null)
                {
                    _mockRepositoryPartialMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("PartialMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryPartialMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryPartialMultiMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryPartialMultiMockSymbols
        {
            get
            {
                if (_mockRepositoryPartialMultiMockSymbols is null)
                {
                    _mockRepositoryPartialMultiMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("PartialMultiMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryPartialMultiMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryGeneratePartialMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryGeneratePartialMockSymbols
        {
            get
            {
                if (_mockRepositoryGeneratePartialMockSymbols is null)
                {
                    _mockRepositoryGeneratePartialMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("GeneratePartialMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryGeneratePartialMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryStrictMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryStrictMockSymbols
        {
            get
            {
                if (_mockRepositoryStrictMockSymbols is null)
                {
                    _mockRepositoryStrictMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("StrictMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryStrictMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryStrictMultiMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryStrictMultiMockSymbols
        {
            get
            {
                if (_mockRepositoryStrictMultiMockSymbols is null)
                {
                    _mockRepositoryStrictMultiMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("StrictMultiMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryStrictMultiMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _mockRepositoryGenerateStrictMockSymbols;

        public IReadOnlyList<ISymbol> MockRepositoryGenerateStrictMockSymbols
        {
            get
            {
                if (_mockRepositoryGenerateStrictMockSymbols is null)
                {
                    _mockRepositoryGenerateStrictMockSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("GenerateStrictMock")
                        .ToList()
                        .AsReadOnly();
                }

                return _mockRepositoryGenerateStrictMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _verifyAllSymbols;

        public IReadOnlyList<ISymbol> VerifyAllSymbols
        {
            get
            {
                if (_verifyAllSymbols is null)
                {
                    _verifyAllSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("VerifyAll")
                        .ToList()
                        .AsReadOnly();
                }

                return _verifyAllSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _replayAllSymbols;

        public IReadOnlyList<ISymbol> ReplayAllSymbols
        {
            get
            {
                if (_replayAllSymbols is null)
                {
                    _replayAllSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("ReplayAll")
                        .ToList()
                        .AsReadOnly();
                }

                return _replayAllSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _backToRecordAllSymbols;

        public IReadOnlyList<ISymbol> BackToRecordAllSymbols
        {
            get
            {
                if (_backToRecordAllSymbols is null)
                {
                    _backToRecordAllSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("BackToRecordAll")
                        .ToList()
                        .AsReadOnly();
                }

                return _backToRecordAllSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _orderedSymbols;

        public IReadOnlyList<ISymbol> OrderedSymbols
        {
            get
            {
                if (_orderedSymbols is null)
                {
                    _orderedSymbols = RhinoMocksMockRepositorySymbol
                        .GetMembers("Ordered")
                        .ToList()
                        .AsReadOnly();
                }

                return _orderedSymbols;
            }
        }

        #endregion

        #region Arguments

        #region Arg

        private IReadOnlyList<ISymbol>? _argIsSymbols;

        public IReadOnlyList<ISymbol> ArgIsSymbols
        {
            get
            {
                if (_argIsSymbols is null)
                {
                    _argIsSymbols = RhinoMocksArgSymbol
                        .GetMembers("Is")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsSymbols;
            }
        }

        #endregion

        #region ArgList

        private IReadOnlyList<ISymbol>? _argListEqualSymbols;

        public IReadOnlyList<ISymbol> ArgListEqualSymbols
        {
            get
            {
                if (_argListEqualSymbols is null)
                {
                    _argListEqualSymbols = RhinoMocksConstraintsListArgSymbol
                        .GetMembers("Equal")
                        .ToList()
                        .AsReadOnly();
                }

                return _argListEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argListIsInSymbols;

        public IReadOnlyList<ISymbol> ArgListIsInSymbols
        {
            get
            {
                if (_argListIsInSymbols is null)
                {
                    _argListIsInSymbols = RhinoMocksConstraintsListArgSymbol
                        .GetMembers("IsIn")
                        .ToList()
                        .AsReadOnly();
                }

                return _argListIsInSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argListContainsAll;

        public IReadOnlyList<ISymbol> ArgListContainsAll
        {
            get
            {
                if (_argListContainsAll is null)
                {
                    _argListContainsAll = RhinoMocksConstraintsListArgSymbol
                        .GetMembers("ContainsAll")
                        .ToList()
                        .AsReadOnly();
                }

                return _argListContainsAll;
            }
        }

        #endregion

        #region Arg<T>

        private IReadOnlyList<ISymbol>? _argMatchesSymbols;

        public IReadOnlyList<ISymbol> ArgMatchesSymbols
        {
            get
            {
                if (_argMatchesSymbols is null)
                {
                    _argMatchesSymbols = RhinoMocksGenericArgSymbol
                        .GetMembers("Matches")
                        .ToList()
                        .AsReadOnly();
                }

                return _argMatchesSymbols;
            }
        }

        #endregion

        #region ArgIs

        private IReadOnlyList<ISymbol>? _argIsAnythingSymbols;

        public IReadOnlyList<ISymbol> ArgIsAnythingSymbols
        {
            get
            {
                if (_argIsAnythingSymbols is null)
                {
                    _argIsAnythingSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("Anything")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsAnythingSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsEqualSymbols;

        public IReadOnlyList<ISymbol> ArgIsEqualSymbols
        {
            get
            {
                if (_argIsEqualSymbols is null)
                {
                    _argIsEqualSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("Equal")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsNotEqualSymbols;

        public IReadOnlyList<ISymbol> ArgIsNotEqualSymbols
        {
            get
            {
                if (_argIsNotEqualSymbols is null)
                {
                    _argIsNotEqualSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("NotEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsNotEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsSameSymbols;

        public IReadOnlyList<ISymbol> ArgIsSameSymbols
        {
            get
            {
                if (_argIsSameSymbols is null)
                {
                    _argIsSameSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("Same")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsSameSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsNotSameSymbols;

        public IReadOnlyList<ISymbol> ArgIsNotSameSymbols
        {
            get
            {
                if (_argIsNotSameSymbols is null)
                {
                    _argIsNotSameSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("NotSame")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsNotSameSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsNullSymbols;

        public IReadOnlyList<ISymbol> ArgIsNullSymbols
        {
            get
            {
                if (_argIsNullSymbols is null)
                {
                    _argIsNullSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("Null")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsNullSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsNotNullSymbols;

        public IReadOnlyList<ISymbol> ArgIsNotNullSymbols
        {
            get
            {
                if (_argIsNotNullSymbols is null)
                {
                    _argIsNotNullSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("NotNull")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsNotNullSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsGreaterThanSymbols;

        public IReadOnlyList<ISymbol> ArgIsGreaterThanSymbols
        {
            get
            {
                if (_argIsGreaterThanSymbols is null)
                {
                    _argIsGreaterThanSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("GreaterThan")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsGreaterThanSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argIsGreaterThanOrEqualSymbols;

        public IReadOnlyList<ISymbol> ArgIsGreaterThanOrEqualSymbols
        {
            get
            {
                if (_argIsGreaterThanOrEqualSymbols is null)
                {
                    _argIsGreaterThanOrEqualSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("GreaterThanOrEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _argIsGreaterThanOrEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argLessThanSymbols;

        public IReadOnlyList<ISymbol> ArgIsLessThanSymbols
        {
            get
            {
                if (_argLessThanSymbols is null)
                {
                    _argLessThanSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("LessThan")
                        .ToList()
                        .AsReadOnly();
                }

                return _argLessThanSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _argLessThanOrEqualSymbols;

        public IReadOnlyList<ISymbol> ArgIsLessThaOrEqualSymbols
        {
            get
            {
                if (_argLessThanOrEqualSymbols is null)
                {
                    _argLessThanOrEqualSymbols = RhinoMocksConstraintsIsArgSymbol
                        .GetMembers("LessThanOrEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _argLessThanOrEqualSymbols;
            }
        }

        #endregion

        #region ArgText

        private IReadOnlyList<ISymbol>? _argTextLikeSymbols;

        public IReadOnlyList<ISymbol> ArgTextLikeSymbols
        {
            get
            {
                if (_argTextLikeSymbols is null)
                {
                    _argTextLikeSymbols = RhinoMocksArgTextSymbol
                        .GetMembers("Like")
                        .ToList()
                        .AsReadOnly();
                }

                return _argTextLikeSymbols;
            }
        }

        #endregion

        #endregion

        #region Constraints

        #region Is

        private IReadOnlyList<ISymbol>? _constraintIsEqualSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsEqualSymbols
        {
            get
            {
                if (_constraintIsEqualSymbols is null)
                {
                    _constraintIsEqualSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("Equal")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsNotEqualSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsNotEqualSymbols
        {
            get
            {
                if (_constraintIsNotEqualSymbols is null)
                {
                    _constraintIsNotEqualSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("NotEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsNotEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsSameSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsSameSymbols
        {
            get
            {
                if (_constraintIsSameSymbols is null)
                {
                    _constraintIsSameSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("Same")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsSameSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsNotSameSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsNotSameSymbols
        {
            get
            {
                if (_constraintIsNotSameSymbols is null)
                {
                    _constraintIsNotSameSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("NotSame")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsNotSameSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsNullSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsNullSymbols
        {
            get
            {
                if (_constraintIsNullSymbols is null)
                {
                    _constraintIsNullSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("Null")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsNullSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsNotNullSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsNotNullSymbols
        {
            get
            {
                if (_constraintIsNotNullSymbols is null)
                {
                    _constraintIsNotNullSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("NotNull")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsNotNullSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsGreaterThanSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsGreaterThanSymbols
        {
            get
            {
                if (_constraintIsGreaterThanSymbols is null)
                {
                    _constraintIsGreaterThanSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("GreaterThan")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsGreaterThanSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsGreaterThanOrEqualSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsGreaterThanOrEqualSymbols
        {
            get
            {
                if (_constraintIsGreaterThanOrEqualSymbols is null)
                {
                    _constraintIsGreaterThanOrEqualSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("GreaterThanOrEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsGreaterThanOrEqualSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsLessThanSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsLessThanSymbols
        {
            get
            {
                if (_constraintIsLessThanSymbols is null)
                {
                    _constraintIsLessThanSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("LessThan")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsLessThanSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintIsLessThanOrEqualSymbols;

        public IReadOnlyList<ISymbol> ConstraintIsLessThanOrEqualSymbols
        {
            get
            {
                if (_constraintIsLessThanOrEqualSymbols is null)
                {
                    _constraintIsLessThanOrEqualSymbols = RhinoMocksConstraintIsSymbol
                        .GetMembers("LessThanOrEqual")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintIsLessThanOrEqualSymbols;
            }
        }

        #endregion

        #region List

        private IReadOnlyList<ISymbol>? _constraintListIsInSymbols;

        public IReadOnlyList<ISymbol> ConstraintListIsInSymbols
        {
            get
            {
                if (_constraintListIsInSymbols is null)
                {
                    _constraintListIsInSymbols = RhinoMocksConstraintListSymbol
                        .GetMembers("IsIn")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintListIsInSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintListContainsAllSymbols;

        public IReadOnlyList<ISymbol> ConstraintListContainsAllSymbols
        {
            get
            {
                if (_constraintListContainsAllSymbols is null)
                {
                    _constraintListContainsAllSymbols = RhinoMocksConstraintListSymbol
                        .GetMembers("ContainsAll")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintListContainsAllSymbols;
            }
        }

        #endregion

        #region Property

        private IReadOnlyList<ISymbol>? _constraintPropertyValueSymbols;

        public IReadOnlyList<ISymbol> ConstraintPropertyValueSymbols
        {
            get
            {
                if (_constraintPropertyValueSymbols is null)
                {
                    _constraintPropertyValueSymbols = RhinoMocksConstraintPropertySymbol
                        .GetMembers("Value")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintPropertyValueSymbols;
            }
        }

        #endregion

        #endregion

        #region RhinoMocksExtension

        private IReadOnlyList<ISymbol>? _expectSymbols;

        public IReadOnlyList<ISymbol> ExpectSymbols
        {
            get
            {
                if (_expectSymbols is null)
                {
                    _expectSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("Expect")
                        .ToList()
                        .AsReadOnly();
                }

                return _expectSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _stubSymbols;

        public IReadOnlyList<ISymbol> StubSymbols
        {
            get
            {
                if (_stubSymbols is null)
                {
                    _stubSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("Stub")
                        .ToList()
                        .AsReadOnly();
                }

                return _stubSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _verifyAllExpectationsSymbols;

        public IReadOnlyList<ISymbol> VerifyAllExpectationsSymbols
        {
            get
            {
                if (_verifyAllExpectationsSymbols is null)
                {
                    _verifyAllExpectationsSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("VerifyAllExpectations")
                        .ToList()
                        .AsReadOnly();
                }

                return _verifyAllExpectationsSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _assertWasNotCalledSymbols;

        public IReadOnlyList<ISymbol> AssertWasNotCalledSymbols
        {
            get
            {
                if (_assertWasNotCalledSymbols is null)
                {
                    _assertWasNotCalledSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("AssertWasNotCalled")
                        .ToList()
                        .AsReadOnly();
                }

                return _assertWasNotCalledSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _assertWasCalledSymbols;

        public IReadOnlyList<ISymbol> AssertWasCalledSymbols
        {
            get
            {
                if (_assertWasCalledSymbols is null)
                {
                    _assertWasCalledSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("AssertWasCalled")
                        .ToList()
                        .AsReadOnly();
                }

                return _assertWasCalledSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _backToRecordSymbols;

        public IReadOnlyList<ISymbol> BackToRecordSymbols
        {
            get
            {
                if (_backToRecordSymbols is null)
                {
                    _backToRecordSymbols = RhinoMocksExtensionSymbol
                        .GetMembers("BackToRecord")
                        .ToList()
                        .AsReadOnly();
                }

                return _backToRecordSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _replaySymbols;

        public IReadOnlyList<ISymbol> ReplaySymbols
        {
            get
            {
                if (_replaySymbols is null)
                {
                    _replaySymbols = RhinoMocksExtensionSymbol
                        .GetMembers("Replay")
                        .ToList()
                        .AsReadOnly();
                }

                return _replaySymbols;
            }
        }

        #endregion

        #region IMethodOptions

        private IReadOnlyList<ISymbol>? _whenCalledSymbols;

        public IReadOnlyList<ISymbol> WhenCalledSymbols
        {
            get
            {
                if (_whenCalledSymbols is null)
                {
                    _whenCalledSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("WhenCalled")
                        .ToList()
                        .AsReadOnly();
                }

                return _whenCalledSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _doSymbols;

        public IReadOnlyList<ISymbol> DoSymbols
        {
            get
            {
                if (_doSymbols is null)
                {
                    _doSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Do")
                        .ToList()
                        .AsReadOnly();
                }

                return _doSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _returnSymbols;

        public IReadOnlyList<ISymbol> ReturnSymbols
        {
            get
            {
                if (_returnSymbols is null)
                {
                    _returnSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Return")
                        .ToList()
                        .AsReadOnly();
                }

                return _returnSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _throwSymbols;

        public IReadOnlyList<ISymbol> ThrowSymbols
        {
            get
            {
                if (_throwSymbols is null)
                {
                    _throwSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Throw")
                        .ToList()
                        .AsReadOnly();
                }

                return _throwSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _callbackSymbols;

        public IReadOnlyList<ISymbol> CallbackSymbols
        {
            get
            {
                if (_callbackSymbols is null)
                {
                    _callbackSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Callback")
                        .ToList()
                        .AsReadOnly();
                }

                return _callbackSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _ignoreArgumentsSymbols;

        public IReadOnlyList<ISymbol> IgnoreArgumentsSymbols
        {
            get
            {
                if (_ignoreArgumentsSymbols is null)
                {
                    _ignoreArgumentsSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("IgnoreArguments")
                        .ToList()
                        .AsReadOnly();
                }

                return _ignoreArgumentsSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _repeatSymbols;

        public IReadOnlyList<ISymbol> RepeatSymbols
        {
            get
            {
                if (_repeatSymbols is null)
                {
                    _repeatSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Repeat")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _callOriginalMethodSymbols;

        public IReadOnlyList<ISymbol> CallOriginalMethodSymbols
        {
            get
            {
                if (_callOriginalMethodSymbols is null)
                {
                    _callOriginalMethodSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("CallOriginalMethod")
                        .ToList()
                        .AsReadOnly();
                }

                return _callOriginalMethodSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _constraintsSymbols;

        public IReadOnlyList<ISymbol> ConstraintsSymbols
        {
            get
            {
                if (_constraintsSymbols is null)
                {
                    _constraintsSymbols = RhinoMocksIMethodOptionsSymbol
                        .GetMembers("Constraints")
                        .ToList()
                        .AsReadOnly();
                }

                return _constraintsSymbols;
            }
        }

        #endregion

        #region IRepeat

        private IReadOnlyList<ISymbol>? _repeatNeverSymbols;

        public IReadOnlyList<ISymbol> RepeatNeverSymbols
        {
            get
            {
                if (_repeatNeverSymbols is null)
                {
                    _repeatNeverSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers("Never")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatNeverSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _repeatOnceSymbols;

        public IReadOnlyList<ISymbol> RepeatOnceSymbols
        {
            get
            {
                if (_repeatOnceSymbols is null)
                {
                    _repeatOnceSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers("Once")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatOnceSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _repeatTwiceSymbols;

        public IReadOnlyList<ISymbol> RepeatTwiceSymbols
        {
            get
            {
                if (_repeatTwiceSymbols is null)
                {
                    _repeatTwiceSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers("Twice")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatTwiceSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _repeatAtLeastOnceSymbols;

        public IReadOnlyList<ISymbol> RepeatAtLeastOnceSymbols
        {
            get
            {
                if (_repeatAtLeastOnceSymbols is null)
                {
                    _repeatAtLeastOnceSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers("AtLeastOnce")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatAtLeastOnceSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _repeatTimesSymbols;

        public IReadOnlyList<ISymbol> RepeatTimesSymbols
        {
            get
            {
                if (_repeatTimesSymbols is null)
                {
                    _repeatTimesSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers("Times")
                        .ToList()
                        .AsReadOnly();
                }

                return _repeatTimesSymbols;
            }
        }

        #endregion

        #region Expect

        private IReadOnlyList<ISymbol>? _expectCallSymbols;

        public IReadOnlyList<ISymbol> ExpectCallSymbols
        {
            get
            {
                if (_expectCallSymbols is null)
                {
                    _expectCallSymbols = RhinoMocksExpectSymbol
                        .GetMembers("Call")
                        .ToList()
                        .AsReadOnly();
                }

                return _expectCallSymbols;
            }
        }

        #endregion

        #region SetupResult

        private IReadOnlyList<ISymbol>? _setupResultForSymbols;

        public IReadOnlyList<ISymbol> SetupResultForSymbols
        {
            get
            {
                if (_setupResultForSymbols is null)
                {
                    _setupResultForSymbols = RhinoMocksSetupResultSymbol
                        .GetMembers("For")
                        .ToList()
                        .AsReadOnly();
                }

                return _setupResultForSymbols;
            }
        }

        #endregion

        #region RhinoMocksSymbol Collections

        private IReadOnlyList<ISymbol>? _allGenerateMockAndStubSymbols;

        public IReadOnlyList<ISymbol> AllGenerateMockAndStubSymbols
        {
            get
            {
                if (_allGenerateMockAndStubSymbols is null)
                {
                    _allGenerateMockAndStubSymbols = MockRepositoryGenerateMockSymbols
                        .Concat(MockRepositoryGenerateStubSymbols)
                        .Concat(MockRepositoryDynamicMockSymbols)
                        .Concat(MockRepositoryDynamicMultiMockSymbols)
                        .Concat(MockRepositoryStubSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allGenerateMockAndStubSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allPartialMockSymbols;

        public IReadOnlyList<ISymbol> AllPartialMockSymbols
        {
            get
            {
                if (_allPartialMockSymbols is null)
                {
                    _allPartialMockSymbols = MockRepositoryPartialMockSymbols
                        .Concat(MockRepositoryPartialMultiMockSymbols)
                        .Concat(MockRepositoryGeneratePartialMockSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allPartialMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allStrictMockSymbols;

        public IReadOnlyList<ISymbol> AllStrictMockSymbols
        {
            get
            {
                if (_allStrictMockSymbols is null)
                {
                    _allStrictMockSymbols = MockRepositoryStrictMockSymbols
                        .Concat(MockRepositoryStrictMultiMockSymbols)
                        .Concat(MockRepositoryGenerateStrictMockSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allStrictMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allMockRepositorySymbols;

        public IReadOnlyList<ISymbol> AllMockRepositorySymbols
        {
            get
            {
                if (_allMockRepositorySymbols is null)
                {
                    _allMockRepositorySymbols = AllGenerateMockAndStubSymbols
                        .Concat(AllStrictMockSymbols)
                        .Concat(AllPartialMockSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allMockRepositorySymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allMockSymbols;

        public IReadOnlyList<ISymbol> AllMockSymbols
        {
            get
            {
                if (_allMockSymbols is null)
                {
                    return _allMockSymbols = AllStrictMockSymbols
                        .Concat(AllPartialMockSymbols)
                        .Concat(MockRepositoryDynamicMultiMockSymbols)
                        .Concat(MockRepositoryDynamicMockSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allMockSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allIRepeatSymbols;

        public IReadOnlyList<ISymbol> AllIRepeatSymbols
        {
            get
            {
                if (_allIRepeatSymbols is null)
                {
                    _allIRepeatSymbols = RhinoMocksIRepeatSymbol
                        .GetMembers()
                        .ToList()
                        .AsReadOnly();
                }

                return _allIRepeatSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allVerifySymbols;

        public IReadOnlyList<ISymbol> AllVerifySymbols
        {
            get
            {
                if (_allVerifySymbols is null)
                {
                    _allVerifySymbols = VerifyAllSymbols
                        .Concat(VerifyAllExpectationsSymbols)
                        .Concat(AssertWasCalledSymbols)
                        .Concat(AssertWasNotCalledSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allVerifySymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _obsoleteRhinoMocksSymbols;

        public IReadOnlyList<ISymbol> ObsoleteRhinoMocksSymbols
        {
            get
            {
                if (_obsoleteRhinoMocksSymbols is null)
                {
                    _obsoleteRhinoMocksSymbols = ReplaySymbols
                        .Concat(ReplayAllSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _obsoleteRhinoMocksSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allIMethodOptionsSymbols;

        public IReadOnlyList<ISymbol> AllIMethodOptionsSymbols
        {
            get
            {
                if (_allIMethodOptionsSymbols is null)
                {
                    _allIMethodOptionsSymbols = ReturnSymbols
                        .Concat(WhenCalledSymbols)
                        .Concat(CallbackSymbols)
                        .Concat(DoSymbols)
                        .Concat(RepeatSymbols)
                        .Concat(ThrowSymbols)
                        .Concat(CallOriginalMethodSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allIMethodOptionsSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allCallbackSymbols;

        public IReadOnlyList<ISymbol> AllCallbackSymbols
        {
            get
            {
                if (_allCallbackSymbols is null)
                {
                    _allCallbackSymbols = WhenCalledSymbols
                        .Concat(CallbackSymbols)
                        .Concat(DoSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allCallbackSymbols;
            }
        }

        private IReadOnlyList<ISymbol>? _allBackToRecordSymbols;

        public IReadOnlyList<ISymbol> AllBackToRecordSymbols
        {
            get
            {
                if (_allBackToRecordSymbols is null)
                {
                    _allBackToRecordSymbols = BackToRecordSymbols
                        .Concat(BackToRecordAllSymbols)
                        .ToList()
                        .AsReadOnly();
                }

                return _allBackToRecordSymbols;
            }
        }

        #endregion
    }
}