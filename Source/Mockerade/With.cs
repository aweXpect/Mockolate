using System;

namespace Mockerade;

/// <summary>
///     Allows the specification of a matching condition for an argument in a method invocation or setup.
/// </summary>
public static class With
{
	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	public static Parameter<T> Any<T>() => new AnyParameter<T>();

	/// <summary>
	///     Matches parameters of type <typeparamref name="T" />, if the <paramref name="predicate" /> returns
	///     <see langword="true" />
	/// </summary>
	public static Parameter<T> Matching<T>(Func<T, bool> predicate)
		=> new PredicateParameter<T>(predicate);

	private sealed class PredicateParameter<T>(Func<T, bool> predicate) : With.Parameter<T>
	{
		protected override bool Matches(T value) => predicate(value);
	}

	/// <summary>
	///     Matches any <see langword="out"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static OutParameter<T> Out<T>(Func<T> setter) => new OutParameter<T>(setter);

	/// <summary>
	///     Matches any <see langword="out"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvocationOutParameter<T> Out<T>() => new InvocationOutParameter<T>();

	/// <summary>
	///     Matches any <see langword="ref"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvocationRefParameter<T> Ref<T>() => new InvocationRefParameter<T>();

	/// <summary>
	///     Matches any <see langword="ref"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, T> setter) => new RefParameter<T>(_ => true, setter);

	/// <summary>
	///     Matches any <see langword="ref"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, bool> predicate, Func<T, T> setter) => new RefParameter<T>(predicate, setter);

	private sealed class AnyParameter<T> : Parameter<T>
	{
		protected override bool Matches(T value) => true;
	}

	/// <summary>
	///     Matches a method parameter against an expectation.
	/// </summary>
	public abstract class Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> matches the expectation;
		///     otherwise <see langword="false" />
		/// </summary>
		public abstract bool Matches(object? value);
	}

	/// <summary>
	///     A named <see cref="Parameter"/>.
	/// </summary>
	/// <param name="Name">The name of the <paramref name="Parameter"/>.</param>
	/// <param name="Parameter">The actual <see cref="Parameter"/>.</param>
	public record NamedParameter(string Name, Parameter Parameter);

	/// <summary>
	///     Matches a method parameter against an expectation.
	/// </summary>
	public abstract class Parameter<T> : Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> is of type <typeparamref name="T" /> and
		///     matches the expectation; otherwise <see langword="false" />
		/// </summary>
		public override bool Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			return value is null && Matches(default!);
		}

		/// <summary>
		///     Verifies the expectation.
		/// </summary>
		protected abstract bool Matches(T value);

		/// <summary>
		///     Implicitly converts to a <see cref="Parameter{T}" /> the compares the <paramref name="value" /> for equality.
		/// </summary>
		public static implicit operator Parameter<T>(T value)
		{
			return new MatchParameterIsEqual(value);
		}

		private sealed class MatchParameterIsEqual : Parameter<T>
		{
			private readonly T _value;

			public MatchParameterIsEqual(T value)
			{
				_value = value;
			}

			protected override bool Matches(T value) => Equals(value, _value);
		}
	}

	/// <summary>
	///     Matches a method <see langword="ref"/> parameter against an expectation.
	/// </summary>
	public class RefParameter<T>(Func<T, bool> predicate, Func<T, T> setter) : Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> is of type <typeparamref name="T" /> and
		///     matches the expectation; otherwise <see langword="false" />
		/// </summary>
		public override bool Matches(object? value)
		{
			return value is T typedValue && predicate(typedValue);
		}

		internal T GetValue(T value) => setter(value);
	}

	/// <summary>
	///     Matches a method <see langword="out"/> parameter against an expectation.
	/// </summary>
	public class OutParameter<T>(Func<T> setter) : Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> is of type <typeparamref name="T" /> and
		///     matches the expectation; otherwise <see langword="false" />
		/// </summary>
		public override bool Matches(object? value)
		{
			return true;
		}

		internal T GetValue() => setter();
	}

	/// <summary>
	///     Matches a method <see langword="out"/> parameter against an expectation.
	/// </summary>
	public class InvocationOutParameter<T>() : Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> is of type <typeparamref name="T" /> and
		///     matches the expectation; otherwise <see langword="false" />
		/// </summary>
		public override bool Matches(object? value)
		{
			return true;
		}
	}

	/// <summary>
	///     Matches a method <see langword="out"/> parameter against an expectation.
	/// </summary>
	public class InvocationRefParameter<T>() : Parameter
	{
		/// <summary>
		///     <see langword="true" />, if the <paramref name="value" /> is of type <typeparamref name="T" /> and
		///     matches the expectation; otherwise <see langword="false" />
		/// </summary>
		public override bool Matches(object? value)
		{
			return true;
		}
	}
}
