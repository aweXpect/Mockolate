using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Mockolate.Analyzers.Tests.Verifiers;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	/// <inheritdoc cref="Microsoft.CodeAnalysis.Diagnostic" />
	public static DiagnosticResult Diagnostic()
		=> CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

	/// <inheritdoc cref="Microsoft.CodeAnalysis.Diagnostic" />
	public static DiagnosticResult Diagnostic(string diagnosticId)
		=> CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

	/// <inheritdoc cref="Microsoft.CodeAnalysis.Diagnostic" />
	public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
		=> CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

	/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])" />
	public static async Task VerifyAnalyzerAsync([StringSyntax("c#-test")] string source,
		params DiagnosticResult[] expected)
	{
		Test test = new()
		{
			TestCode = source,
			TestState =
			{
				AdditionalReferences =
				{
					typeof(Mock<>).Assembly.Location,
				},
			},
		};

		// TODO: Remove once CS1705 no longer occurs
		test.CompilerDiagnostics = CompilerDiagnostics.None;
		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync(CancellationToken.None);
	}
}
