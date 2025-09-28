namespace Mockerade.Invocations;

/// <summary>
///     The expectation contains the matching invocations for verification.
/// </summary>
public class InvocationResult(Invocation[] invocations)
{
	/// <summary>
	///     The matching invocations.
	/// </summary>
	public Invocation[] Invocations { get; } = invocations;

	/// <summary>
	///     A property expectation returns the getter or setter <see cref="InvocationResult"/> for the given <paramref name="propertyName"/>.
	/// </summary>
	public class Property<T>(IMockInvocations mockInvocations, string propertyName)
	{
		/// <summary>
		/// The expectation for the property getter invocations.
		/// </summary>
		public InvocationResult Getter() => new InvocationResult(mockInvocations.PropertyGetter(propertyName));
		/// <summary>
		/// The expectation for the property setter invocations matching the specified <paramref name="value"/>.
		/// </summary>
		public InvocationResult Setter(With.Parameter<T> value) => new InvocationResult(mockInvocations.PropertySetter(propertyName, value));
	}
}
