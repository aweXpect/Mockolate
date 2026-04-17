using System;
using System.Diagnostics;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for method setups.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class MethodSetup : IMethodSetup, IVerifiableMethodSetup
{
	/// <summary>
	///     Base class for method setups.
	/// </summary>
	protected MethodSetup(string name)
	{
		Name = name;
	}

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
	protected virtual bool MatchesInteraction(IMethodInteraction interaction) => false;
}
