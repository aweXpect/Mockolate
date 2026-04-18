using System.Diagnostics;
using Mockolate.Internals;

namespace Mockolate.Interactions;

/// <summary>
///     An invocation of a method without parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MethodInvocation(string name) : IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}()";
}

/// <summary>
///     An invocation of a method with a single <paramref name="parameter1" />.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public struct MethodInvocation<T1>(string name, string parameterName1, T1 parameter1) : IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The single parameter name of the method.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The single parameter value of the method.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({Parameter1?.ToString() ?? "null"})";
}

/// <summary>
///     An invocation of a method with two parameters <paramref name="parameter1" /> and <paramref name="parameter2" />.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MethodInvocation<T1, T2>(string name, string parameterName1, T1 parameter1, string parameterName2, T2 parameter2) : IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The first parameter name of the method.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the method.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the method.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the method.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"})";
}

/// <summary>
///     An invocation of a method with three parameters <paramref name="parameter1" />, <paramref name="parameter2" /> and
///     <paramref name="parameter3" />.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MethodInvocation<T1, T2, T3>(string name, string parameterName1, T1 parameter1, string parameterName2, T2 parameter2, string parameterName3, T3 parameter3)
	: IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The first parameter name of the method.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the method.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the method.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the method.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <summary>
	///     The third parameter name of the method.
	/// </summary>
	public string ParameterName3 { get; } = parameterName3;

	/// <summary>
	///     The third parameter value of the method.
	/// </summary>
	public T3 Parameter3 { get; } = parameter3;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"}, {Parameter3?.ToString() ?? "null"})";
}

/// <summary>
///     An invocation of a method with four parameters <paramref name="parameter1" />, <paramref name="parameter2" />,
///     <paramref name="parameter3" /> and <paramref name="parameter4" />.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MethodInvocation<T1, T2, T3, T4>(string name, string parameterName1, T1 parameter1, string parameterName2, T2 parameter2, string parameterName3, T3 parameter3, string parameterName4, T4 parameter4)
	: IMethodInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The first parameter name of the method.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the method.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the method.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the method.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <summary>
	///     The third parameter name of the method.
	/// </summary>
	public string ParameterName3 { get; } = parameterName3;

	/// <summary>
	///     The third parameter value of the method.
	/// </summary>
	public T3 Parameter3 { get; } = parameter3;

	/// <summary>
	///     The fourth parameter name of the method.
	/// </summary>
	public string ParameterName4 { get; } = parameterName4;

	/// <summary>
	///     The fourth parameter value of the method.
	/// </summary>
	public T4 Parameter4 { get; } = parameter4;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"}, {Parameter3?.ToString() ?? "null"}, {Parameter4?.ToString() ?? "null"})";
}
