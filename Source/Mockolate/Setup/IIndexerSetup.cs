using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="IndexerSetup" />.
/// </summary>
public interface IIndexerSetup
{
	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	bool Matches(IInteraction invocation);

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value"/> for the <paramref name="parameters"/>, if an initialization is set up.
	/// </summary>
	bool TryGetInitialValue<TValue>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out TValue value);
}
