#if NET9_0_OR_GREATER
using System.Diagnostics;
using Mockolate.Internals;

namespace Mockolate.Interactions;

/// <summary>
///     An invocation of a method whose parameter is a ref struct.
/// </summary>
/// <remarks>
///     <para>
///         Ref struct values cannot be boxed or stored as fields on a heap object, so only the
///         method name and parameter name are retained. This is a structurally separate interaction
///         type from <see cref="MethodInvocation{T}" /> because a class field of ref struct type is
///         forbidden by the type system.
///     </para>
///     <para>
///         As a consequence, post-hoc verification of the parameter's value is not supported for
///         ref struct parameters — matchers passed to <c>Verify</c> apply at call time only, not at
///         verify time. Callers that need recorded metadata should perform their own capture
///         inside the setup (e.g. projecting a scalar) rather than relying on the interaction log.
///     </para>
/// </remarks>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class RefStructMethodInvocation(string name, string parameterName) : IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The name of the ref struct parameter.
	/// </summary>
	public string ParameterName { get; } = parameterName;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({ParameterName}: <ref struct>)";
}
#endif
