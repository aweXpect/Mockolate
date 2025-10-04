using System.Collections.Generic;

namespace Mockolate.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvocations
{
	private readonly List<IInvocation> _invocations = [];

	/// <summary>
	///     Indicates whether (at least) one invocation was already triggered.
	/// </summary>
	public bool IsAlreadyInvoked => _invocations.Count > 0;

	/// <summary>
	///     The number of invocations contained in the collection.
	/// </summary>
	public int Count => _invocations.Count;

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IEnumerable<IInvocation> Invocations => _invocations;

	internal IInvocation RegisterInvocation(IInvocation invocation)
	{
		_invocations.Add(invocation);
		return invocation;
	}
}
