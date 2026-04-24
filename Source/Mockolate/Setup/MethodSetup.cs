using System;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for method setups.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public abstract class MethodSetup : IMethodSetup, IVerifiableMethodSetup
{
	/// <summary>
	///     Base class for method setups.
	/// </summary>
	protected MethodSetup(string name) : this(-1, name)
	{
	}

	/// <summary>
	///     Base class for method setups, carrying the generator-assigned <paramref name="memberId" />
	///     used for the fast O(1) setup lookup on the invocation hot path.
	/// </summary>
	protected MethodSetup(int memberId, string name)
	{
		MemberId = memberId;
		Name = name;
	}

	/// <summary>
	///     Generator-assigned dense integer id for the member this setup targets, or <c>-1</c> when the
	///     setup was created through a legacy path. Used by the member-id-indexed setup store for
	///     closure-free dispatch.
	/// </summary>
	public int MemberId { get; }

	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; }

	/// <summary>
	///     Returns a formatted string representation of the given <paramref name="type" />.
	/// </summary>
	protected static string FormatType(Type type)
		=> type.FormatType();

	/// <inheritdoc cref="IVerifiableMethodSetup.Matches(IMethodInteraction)" />
	bool IVerifiableMethodSetup.Matches(IMethodInteraction interaction)
		=> MatchesInteraction(interaction);

	/// <summary>
	///     Checks if the setup matches the method invocations.
	/// </summary>
	protected abstract bool MatchesInteraction(IMethodInteraction interaction);
}
