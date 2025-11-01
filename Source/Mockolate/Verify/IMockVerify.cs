using Mockolate.Interactions;

namespace Mockolate.Verify;

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
