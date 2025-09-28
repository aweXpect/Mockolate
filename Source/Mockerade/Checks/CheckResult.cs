namespace Mockerade.Checks;

/// <summary>
///     The expectation contains the matching invocations for verification.
/// </summary>
public class CheckResult(Invocation[] invocations)
{
	/// <summary>
	///     The matching invocations.
	/// </summary>
	public Invocation[] Invocations { get; } = invocations;

	/// <summary>
	///     A property expectation returns the getter or setter <see cref="CheckResult"/> for the given <paramref name="propertyName"/>.
	/// </summary>
	public class Property<T>(IMockChecks mockInvocations, string propertyName)
	{
		/// <summary>
		/// The expectation for the property getter invocations.
		/// </summary>
		public CheckResult Getter() => new CheckResult(mockInvocations.PropertyGetter(propertyName));
		/// <summary>
		/// The expectation for the property setter invocations matching the specified <paramref name="value"/>.
		/// </summary>
		public CheckResult Setter(With.Parameter<T> value) => new CheckResult(mockInvocations.PropertySetter(propertyName, value));
	}
}
