using Mockolate.Interactions;

namespace Mockolate.V2;

/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="T" />.
/// </summary>
public interface IMockVerify<out T>
{
	/// <summary>
	///     The mock instance.
	/// </summary>
	T Subject { get; }
	
	/// <summary>
	///     The interactions recorded on the mock.
	/// </summary>
	MockInteractions Interactions { get; }
	
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	bool ThatAllInteractionsAreVerified();
}
#pragma warning restore S2326 // Unused type parameters should be removed
