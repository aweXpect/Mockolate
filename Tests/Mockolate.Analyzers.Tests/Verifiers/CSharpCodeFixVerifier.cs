using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Mockolate.Analyzers.Tests.Verifiers;

namespace Mockolate.Analyzers.Tests.Verifiers;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
	{
		public Test()
		{
			SolutionTransforms.Add((solution, projectId) =>
			{
				Project? project = solution.GetProject(projectId);

				if (project is null)
				{
					return solution;
				}

				CompilationOptions? compilationOptions = project.CompilationOptions;

				if (compilationOptions is null)
				{
					return solution;
				}

				CSharpParseOptions? parseOptions = project.ParseOptions as CSharpParseOptions;

				if (parseOptions is null)
				{
					return solution;
				}

				compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
					compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));

				solution = solution.WithProjectCompilationOptions(projectId, compilationOptions)
					.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.Preview));

				return solution;
			});
		}
	}
}
