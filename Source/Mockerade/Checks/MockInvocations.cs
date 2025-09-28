using System.Collections.Generic;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvocations
{
	/// <summary>
	/// Indicates whether (at least) one invocation was already triggered.
	/// </summary>
	public bool IsAlreadyInvoked => _invocations.Count > 0;

	private readonly List<Invocation> _invocations = [];

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IReadOnlyList<Invocation> Invocations => _invocations.AsReadOnly();

	internal Invocation RegisterInvocation(Invocation invocation)
	{
		_invocations.Add(invocation);
		return invocation;
	}
}
