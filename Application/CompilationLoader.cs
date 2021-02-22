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
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace RhinoMocksToMoqRewriter.Application
{
  public class CompilationLoader
  {
    private readonly MSBuildWorkspace _workspace;

    public CompilationLoader ()
    {
      var instance = MSBuildLocator.QueryVisualStudioInstances().First();
      MSBuildLocator.RegisterInstance (instance);
      _workspace = MSBuildWorkspace.Create();
    }

    public async Task<Solution> LoadSolutionAsync (string pathToSolution)
    {
      return await _workspace.OpenSolutionAsync (pathToSolution);
    }

    public async Task<Project> LoadProjectAsync (string pathToProject)
    {
      return await _workspace.OpenProjectAsync (pathToProject);
    }

    public async Task<IReadOnlyList<CSharpCompilation>> LoadCompilationsAsync (IEnumerable<Project> projects)
    {
      var allCompilations = await Task
          .WhenAll (projects.Select (async p => await p.GetCompilationAsync()));
      var rhinoMocksCompilations = allCompilations
          .Select (c => c as CSharpCompilation)
          .Where (c => c?.ReferencedAssemblyNames.Any (a => a.Name.Contains ("Rhino.Mocks")) == true);

      return rhinoMocksCompilations.ToList().AsReadOnly()!;
    }
  }
}