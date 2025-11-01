using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;

namespace Mockolate.Analyzers.Tests.Verifiers;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
	where TCodeRefactoring : CodeRefactoringProvider, new()
{
	/// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, string)" />
	public static async Task VerifyRefactoringAsync(string source, string fixedSource)
		=> await VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

	/// <inheritdoc
	///     cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult, string)" />
	public static async Task VerifyRefactoringAsync(string source, DiagnosticResult expected, string fixedSource)
		=> await VerifyRefactoringAsync(source, [expected,], fixedSource);

	/// <inheritdoc
	///     cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult[], string)" />
	public static async Task VerifyRefactoringAsync(string source, DiagnosticResult[] expected, string fixedSource)
	{
		Test test = new()
		{
			TestCode = source,
			FixedCode = fixedSource,
		};

		// TODO: Remove once CS1705 no longer occurs
		test.CompilerDiagnostics = CompilerDiagnostics.None;
		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync(CancellationToken.None);
	}
}
