using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="IndexerSetup" />.
/// </summary>
public interface IIndexerSetup
{
	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	bool? CallBaseClass();
	
	/// <summary>
	///     Checks if the <paramref name="indexerAccess" /> matches the setup.
	/// </summary>
	bool Matches(IndexerAccess indexerAccess);

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value" /> for the <paramref name="parameters" />, if an
	///     initialization is set up.
	/// </summary>
	bool TryGetInitialValue<TValue>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out TValue value);
}
