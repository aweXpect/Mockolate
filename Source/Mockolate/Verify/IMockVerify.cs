using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Verifies the <see cref="Interactions"/> with the mocked subject in the <typeparamref name="TMock"/> <see cref="Mock"/>.
/// </summary>
public interface IMockVerify<TMock>
{
	/// <summary>
	///     The interactions recorded on the mock.
	/// </summary>
	MockInteractions Interactions { get; }

	/// <summary>
	///     The mock instance.
	/// </summary>
	TMock Mock { get; }
}
