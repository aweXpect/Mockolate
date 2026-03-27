using System.ComponentModel;
using System.Diagnostics;

namespace Mockolate.Parameters;

/// <summary>
///     Wraps either a concrete value or a parameter matcher for use in setup and verify calls.
///     Supports implicit conversion from both concrete values and <see cref="ParameterMatcher" /> instances.
/// </summary>
[DebuggerNonUserCode]
public readonly struct Param<T>
{
	private readonly IParameter _inner;

	internal Param(IParameter inner) => _inner = inner;

	/// <summary>
	///     Returns the underlying <see cref="IParameter" />.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public IParameter ToParameter() => _inner;

	/// <summary>
	///     Implicitly converts a concrete value to a <see cref="Param{T}" />.
	/// </summary>
	public static implicit operator Param<T>(T value)
		=> new((IParameter)It.Is(value));

	/// <summary>
	///     Implicitly converts a typed <see cref="ParameterMatcher{T}" /> to a <see cref="Param{T}" />.
	///     This is preferred over the non-generic conversion when the type parameter matches,
	///     which resolves overload ambiguity for indexers and generic methods.
	/// </summary>
	public static implicit operator Param<T>(ParameterMatcher<T> matcher)
		=> new(matcher);

	/// <summary>
	///     Implicitly converts any <see cref="ParameterMatcher" /> to a <see cref="Param{T}" />.
	///     This uses the non-generic base class to allow any matcher (regardless of its generic type parameter)
	///     to be accepted, enabling scenarios like covariant matching.
	/// </summary>
	public static implicit operator Param<T>(ParameterMatcher matcher)
		=> new(matcher);
}

/// <summary>
///     Extension methods for converting <see cref="IParameter{T}" /> to <see cref="Param{T}" />.
/// </summary>
[DebuggerNonUserCode]
public static class ParamExtensions
{
	/// <summary>
	///     Wraps an <see cref="IParameter{T}" /> into a <see cref="Param{T}" />.
	///     Useful for covariance scenarios where <typeparamref name="T" /> differs from the matcher's type parameter.
	/// </summary>
	/// <example>
	///     <code>
	///     // Covariant matching: matcher targets MyImpl, but setup expects IMyBase
	///     sut.Mock.Setup.DoSomething(It.Satisfies&lt;MyImpl&gt;(t => t.Prop == true).AsParam&lt;IMyBase&gt;());
	///     </code>
	/// </example>
	public static Param<T> AsParam<T>(this IParameter<T> parameter)
		=> new((IParameter)parameter);
}

