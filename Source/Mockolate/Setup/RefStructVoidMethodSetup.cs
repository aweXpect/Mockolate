#if NET9_0_OR_GREATER
using System;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Concrete ref-struct-compatible setup for a <see langword="void" /> method with a single
///     ref struct parameter of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     <para>
///         Stores no field of type <typeparamref name="T" /> and no <c>Action&lt;T&gt;</c> —
///         both are illegal for a ref struct <typeparamref name="T" />. The setup matches calls via
///         an <see cref="IParameterMatch{T}" /> supplied at construction. Throw configuration is
///         stored in a single <see cref="Func{Exception}" /> slot; last <c>Throws</c>/<c>DoesNotThrow</c>
///         call wins. The full <c>Callbacks&lt;T&gt;</c> sequencing machinery used by the existing
///         <see cref="VoidMethodSetup{T1}" /> hierarchy is out of scope — it fundamentally cannot
///         carry a ref struct <typeparamref name="T" /> and would require its own ref-struct-safe
///         redesign.
///     </para>
///     <para>
///         <see cref="MatchesInteraction" /> on the verify side matches by method name only,
///         because the parameter value is not retained in a <see cref="RefStructMethodInvocation" />.
///         See that type's remarks for design rationale.
///     </para>
/// </remarks>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public sealed class RefStructVoidMethodSetup<T> : MethodSetup, IRefStructVoidMethodSetup<T>
	where T : allows ref struct
{
	private readonly IParameterMatch<T>? _matcher;
	private Func<Exception?>? _returnAction;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="RefStructVoidMethodSetup{T}" />
	/// <param name="name">The method name.</param>
	/// <param name="matcher">
	///     The parameter matcher. When <see langword="null" />, the setup matches any call to
	///     <paramref name="name" />.
	/// </param>
	public RefStructVoidMethodSetup(string name, IParameterMatch<T>? matcher = null)
		: base(name)
	{
		_matcher = matcher;
	}

	/// <summary>
	///     Checks whether this setup's matcher accepts the given ref struct <paramref name="value" />.
	/// </summary>
	/// <remarks>
	///     Must be called synchronously from the caller's stack frame — <paramref name="value" />
	///     is a ref struct and cannot be captured in a closure.
	/// </remarks>
	public bool Matches(T value)
		=> _matcher is null || _matcher.Matches(value);

	/// <summary>
	///     Invokes any matcher-level callbacks against the live <paramref name="value" /> and then
	///     applies the currently configured throw (if any).
	/// </summary>
	public void Invoke(T value)
	{
		_matcher?.InvokeCallbacks(value);
		if (_returnAction is null)
		{
			return;
		}

		Exception? exception = _returnAction();
		if (exception is not null)
		{
			throw exception;
		}
	}

	/// <summary>
	///     The flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass(MockBehavior behavior)
		=> _skipBaseClass ?? behavior.SkipBaseClass;

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.SkippingBaseClass(bool)" />
	IRefStructVoidMethodSetup<T> IRefStructVoidMethodSetup<T>.SkippingBaseClass(bool skipBaseClass)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.DoesNotThrow()" />
	IRefStructVoidMethodSetup<T> IRefStructVoidMethodSetup<T>.DoesNotThrow()
	{
		_returnAction = static () => null;
		return this;
	}

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws{TException}()" />
	IRefStructVoidMethodSetup<T> IRefStructVoidMethodSetup<T>.Throws<TException>()
	{
		_returnAction = static () => new TException();
		return this;
	}

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Exception)" />
	IRefStructVoidMethodSetup<T> IRefStructVoidMethodSetup<T>.Throws(Exception exception)
	{
		_returnAction = () => exception;
		return this;
	}

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Func{Exception})" />
	IRefStructVoidMethodSetup<T> IRefStructVoidMethodSetup<T>.Throws(Func<Exception> exceptionFactory)
	{
		_returnAction = exceptionFactory;
		return this;
	}

	/// <inheritdoc cref="MethodSetup.MatchesInteraction(IMethodInteraction)" />
	protected override bool MatchesInteraction(IMethodInteraction interaction)
		=> interaction is RefStructMethodInvocation invocation && invocation.Name == Name;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _matcher is null
			? $"void {Name}(<{typeof(T).Name} ref struct>)"
			: $"void {Name}({_matcher})";
}
#endif
