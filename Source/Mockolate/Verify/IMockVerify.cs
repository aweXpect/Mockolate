using Mockolate.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Get results for property access on the mock.
/// </summary>
public interface IMockVerify<TMock>
{
	/// <summary>
	/// The interactions recorded on the mock.
	/// </summary>
	MockInteractions Interactions { get; }

	/// <summary>
	/// The mock instance.
	/// </summary>
	TMock Mock { get; }
}
