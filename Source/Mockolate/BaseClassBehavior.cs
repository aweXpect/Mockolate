namespace Mockolate;

/// <summary>
///     Specifies the behavior to use when interacting with members of a base class in the mock.
/// </summary>
public enum BaseClassBehavior
{
	/// <summary>
	///     (Default) Does not call the base class implementation.
	/// </summary>
	DoNotCallBaseClass,

	/// <summary>
	///     Calls the base class implementation, but ignores its values.
	/// </summary>
	OnlyCallBaseClass,

	/// <summary>
	///     Calls the base class implementation, and uses its return values as default values.
	/// </summary>
	UseBaseClassAsDefaultValue
}
