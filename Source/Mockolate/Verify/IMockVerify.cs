using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject.
/// </summary>
public interface IMockVerify
{
	/// <summary>
	///     The interactions recorded on the mock.
	/// </summary>
	MockInteractions Interactions { get; }
}

/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="TMock" />
///     <see cref="Mock" />.
/// </summary>
public interface IMockVerify<out TMock> : IMockVerify
{
	/// <summary>
	///     The mock instance.
	/// </summary>
	TMock Mock { get; }
}

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Verifies the interactions with the mocked subject in the <typeparamref name="TMock" />.
/// </summary>
public interface IMockVerify<T, out TMock>
{
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	bool ThatAllInteractionsAreVerified();
}
#pragma warning restore S2326 // Unused type parameters should be removed
