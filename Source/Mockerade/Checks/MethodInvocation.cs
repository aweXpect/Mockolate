namespace Mockerade.Checks;

/// <summary>
///     An invocation of a method.
/// </summary>
public class MethodInvocation(string name, object?[] parameters) : Invocation
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The parameters of the method.
	/// </summary>
	public object?[] Parameters { get; } = parameters;
}
