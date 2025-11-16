namespace Mockolate;

/// <summary>
///     Specifies the behavior to use when interacting with members of a base class in the mock.
/// </summary>
public enum BaseClassBehavior
{
	/// <summary>
	///     (Default) Does not call the base class implementation.
	/// </summary>
	IgnoreBaseClass,

	/// <summary>
	///     Calls the base class implementation, and uses its return values as default values.
	/// </summary>
	CallBaseClass,
}
