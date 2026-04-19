#if NET9_0_OR_GREATER
using Mockolate.Parameters;

namespace Mockolate;

public partial class It
{
	/// <summary>
	///     Matches any value of the ref struct type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     Use instead of <see cref="IsAny{T}" /> for parameter types that require the
	///     <c>allows ref struct</c> anti-constraint. The returned matcher implements
	///     <see cref="IParameter{T}" /> and <see cref="IParameterMatch{T}" /> only — it does not
	///     carry an <c>Action&lt;T&gt;</c> callback slot, which would be illegal for a ref struct
	///     <typeparamref name="T" />.
	/// </remarks>
	public static IParameter<T> IsAnyRefStruct<T>()
		where T : allows ref struct
		=> new AnyRefStructMatch<T>();

	/// <summary>
	///     Matches a ref struct value of type <typeparamref name="T" /> against the given
	///     <paramref name="predicate" />.
	/// </summary>
	public static IParameter<T> IsRefStruct<T>(RefStructPredicate<T> predicate)
		where T : allows ref struct
		=> new RefStructPredicateMatch<T>(predicate);

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class AnyRefStructMatch<T> : IParameter<T>, IParameterMatch<T>
		where T : allows ref struct
	{
		public bool Matches(object? value) => false;
		public bool Matches(T value) => true;
		public void InvokeCallbacks(object? value) { }
		public void InvokeCallbacks(T value) { }

		public override string ToString()
			=> $"It.IsAnyRefStruct<{typeof(T).Name}>()";
	}

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class RefStructPredicateMatch<T>(RefStructPredicate<T> predicate)
		: IParameter<T>, IParameterMatch<T>
		where T : allows ref struct
	{
		public bool Matches(object? value) => false;
		public bool Matches(T value) => predicate(value);
		public void InvokeCallbacks(object? value) { }
		public void InvokeCallbacks(T value) { }

		public override string ToString()
			=> $"It.IsRefStruct<{typeof(T).Name}>(<predicate>)";
	}
}
#endif
