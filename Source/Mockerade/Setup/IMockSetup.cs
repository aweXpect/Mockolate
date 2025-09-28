namespace Mockerade.Setup;

/// <summary>
///     Allows registration of <see cref="MethodSetup" /> in the mock.
/// </summary>
public interface IMockSetup
{
	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	void RegisterMethod(MethodSetup methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	void RegisterProperty(string propertyName, PropertySetup propertySetup);
}
