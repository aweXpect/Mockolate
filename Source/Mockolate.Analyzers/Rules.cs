using Microsoft.CodeAnalysis;

namespace Mockolate.Analyzers;

/// <summary>
///     The rules for mockolate analyzers.
/// </summary>
public static class Rules
{
	private const string UsageCategory = "Usage";

	/// <summary>
	///     Verifications must be used.
	/// </summary>
	public static readonly DiagnosticDescriptor UseVerificationRule =
		CreateDescriptor("Mockolate0001", UsageCategory, DiagnosticSeverity.Error);

	private static DiagnosticDescriptor CreateDescriptor(string diagnosticId, string category,
		DiagnosticSeverity severity) => new(
		diagnosticId,
		new LocalizableResourceString(diagnosticId + "Title",
			Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(diagnosticId + "MessageFormat", Resources.ResourceManager,
			typeof(Resources)),
		category,
		severity,
		true,
		new LocalizableResourceString(diagnosticId + "Description", Resources.ResourceManager,
			typeof(Resources))
	);
}
