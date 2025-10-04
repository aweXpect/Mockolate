using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mockolate.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvocations
{
	/// <summary>
	///     Indicates whether (at least) one invocation was already triggered.
	/// </summary>
	public bool IsAlreadyInvoked => _invocations.Count > 0;

	/// <summary>
	///     The number of invocations contained in the collection.
	/// </summary>
	public int Count => _invocations.Count;

	private readonly List<Invocation> _invocations = [];

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IEnumerable<Invocation> Invocations => _invocations;

	internal Invocation RegisterInvocation(Invocation invocation)
	{
		_invocations.Add(invocation);
		return invocation;
	}
}
