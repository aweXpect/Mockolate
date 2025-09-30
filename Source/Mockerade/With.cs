using System;

namespace Mockerade;

/// <summary>
///     Specify a matching condition for a method parameter.
/// </summary>
public static class With
{
	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>Also matches, if the method parameter is <see langword="null" />.</remarks>
	public static Parameter<T> Any<T>()
		=> new AnyParameter<T>();

	/// <summary>
	///     Matches a parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate" />.
	/// </summary>
	public static Parameter<T> Matching<T>(Func<T, bool> predicate)
		=> new PredicateParameter<T>(predicate);

	/// <summary>
	///     Matches any <see langword="out"/> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter"/> to set the value when the method is invoked.
	/// </summary>
	public static OutParameter<T> Out<T>(Func<T> setter)
		=> new OutParameter<T>(setter);

	/// <summary>
	///     Matches any <see langword="out"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvokedOutParameter<T> Out<T>()
		=> new InvokedOutParameter<T>();

	/// <summary>
	///     Matches any <see langword="ref"/> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter"/> to set the value when the method is invoked.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, T> setter)
		=> new RefParameter<T>(_ => true, setter);

	/// <summary>
	///     Matches a <see langword="ref"/> parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate"/> and
	///     uses the <paramref name="setter"/> to set the value when the method is invoked.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, bool> predicate, Func<T, T> setter)
		=> new RefParameter<T>(predicate, setter);

	/// <summary>
	///     Matches any <see langword="ref"/> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvokedRefParameter<T> Ref<T>()
		=> new InvokedRefParameter<T>();

	/// <summary>
	///     Matches a method parameter against an expectation.
	/// </summary>
	public abstract class Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter; otherwise <see langword="false" />.
		/// </returns>
		public abstract bool Matches(object? value);
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public abstract class Parameter<T> : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			return value is null && Matches(default!);
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value"/>.
		/// </summary>
		protected abstract bool Matches(T value);

		/// <summary>
		///     Implicitly converts to a <see cref="Parameter{T}" /> that compares the <paramref name="value" /> for equality.
		/// </summary>
		public static implicit operator Parameter<T>(T value)
		{
			return new ParameterEquals(value);
		}

		private sealed class ParameterEquals : Parameter<T>
		{
			private readonly T _value;

			public ParameterEquals(T value)
			{
				_value = value;
			}

			protected override bool Matches(T value) => Equals(value, _value);
		}
	}

	/// <summary>
	///     Matches an <see langword="out"/> parameter against an expectation.
	/// </summary>
	public class OutParameter<T>(Func<T> setter) : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value)
		{
			return true;
		}

		/// <summary>
		///     Retrieves the value to which the <see langword="out"/> parameter should be set.
		/// </summary>
		public T GetValue() => setter();
	}

	/// <summary>
	///     Matches any <see langword="out"/> parameter.
	/// </summary>
	public class InvokedOutParameter<T>() : Parameter
	{
		/// <summary>
		///     Matches any <paramref name="value"/>.
		/// </summary>
		public override bool Matches(object? value)
		{
			return true;
		}
	}

	/// <summary>
	///     Matches a method <see langword="ref"/> parameter against an expectation.
	/// </summary>
	public class RefParameter<T>(Func<T, bool> predicate, Func<T, T> setter) : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value)
		{
			return value is T typedValue && predicate(typedValue);
		}

		/// <summary>
		///     Retrieves the value to which the <see langword="ref"/> parameter should be set.
		/// </summary>
		public T GetValue(T value) => setter(value);
	}

	/// <summary>
	///     Matches a method <see langword="out"/> parameter against an expectation.
	/// </summary>
	public class InvokedRefParameter<T>() : Parameter
	{
		/// <summary>
		///     Matches any <paramref name="value"/>.
		/// </summary>
		public override bool Matches(object? value)
		{
			return true;
		}
	}

	/// <summary>
	///     A named <see cref="Parameter"/>.
	/// </summary>
	/// <param name="Name">The name of the <paramref name="Parameter"/>.</param>
	/// <param name="Parameter">The actual <see cref="Parameter"/>.</param>
	public record NamedParameter(string Name, Parameter Parameter);

	private sealed class AnyParameter<T> : Parameter<T>
	{
		protected override bool Matches(T value) => true;
	}

	private sealed class PredicateParameter<T>(Func<T, bool> predicate) : With.Parameter<T>
	{
		protected override bool Matches(T value) => predicate(value);
	}
}
