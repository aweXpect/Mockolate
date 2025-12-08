using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Extensions for indexer, property and method setups.
/// </summary>
public static class SetupExtensions
{
	/// <summary>
	///     Extensions for property setups.
	/// </summary>
	extension<T>(IPropertySetupReturnWhenBuilder<T> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IPropertySetup<T> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for property callback setups.
	/// </summary>
	extension<T>(IPropertySetupWhenBuilder<T> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IPropertySetup<T> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer setups with one parameter.
	/// </summary>
	extension<TValue, T1>(IIndexerSetupReturnBuilder<TValue, T1> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IIndexerSetup<TValue, T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer callback setups with one parameter.
	/// </summary>
	extension<TValue, T1>(IIndexerSetupCallbackBuilder<TValue, T1> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IIndexerSetup<TValue, T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer setups with two parameters.
	/// </summary>
	extension<TValue, T1, T2>(IIndexerSetupReturnBuilder<TValue, T1, T2> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer callback setups with two parameters.
	/// </summary>
	extension<TValue, T1, T2>(IIndexerSetupCallbackBuilder<TValue, T1, T2> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer setups with three parameters.
	/// </summary>
	extension<TValue, T1, T2, T3>(IIndexerSetupReturnBuilder<TValue, T1, T2, T3> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer callback setups with three parameters.
	/// </summary>
	extension<TValue, T1, T2, T3>(IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer setups with four parameters.
	/// </summary>
	extension<TValue, T1, T2, T3, T4>(IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for indexer callback setups with four parameters.
	/// </summary>
	extension<TValue, T1, T2, T3, T4>(IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IIndexerSetup<TValue, T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning <typeparamref name="TReturn" /> without parameters.
	/// </summary>
	extension<TReturn>(IReturnMethodSetupReturnBuilder<TReturn> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IReturnMethodSetup<TReturn> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning <typeparamref name="TReturn" /> without parameters.
	/// </summary>
	extension<TReturn>(IReturnMethodSetupCallbackBuilder<TReturn> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IReturnMethodSetup<TReturn> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning <typeparamref name="TReturn" /> with one parameter.
	/// </summary>
	extension<TReturn, T1>(IReturnMethodSetupReturnBuilder<TReturn, T1> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning <typeparamref name="TReturn" /> with one parameter.
	/// </summary>
	extension<TReturn, T1>(IReturnMethodSetupCallbackBuilder<TReturn, T1> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning <typeparamref name="TReturn" /> with two parameters.
	/// </summary>
	extension<TReturn, T1, T2>(IReturnMethodSetupReturnBuilder<TReturn, T1, T2> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning <typeparamref name="TReturn" /> with two parameters.
	/// </summary>
	extension<TReturn, T1, T2>(IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning <typeparamref name="TReturn" /> with three parameters.
	/// </summary>
	extension<TReturn, T1, T2, T3>(IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning <typeparamref name="TReturn" /> with three parameters.
	/// </summary>
	extension<TReturn, T1, T2, T3>(IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning <typeparamref name="TReturn" /> with four parameters.
	/// </summary>
	extension<TReturn, T1, T2, T3, T4>(IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning <typeparamref name="TReturn" /> with four parameters.
	/// </summary>
	extension<TReturn, T1, T2, T3, T4>(IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IReturnMethodSetup<TReturn, T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}


	/// <summary>
	///     Extensions for method setup returning void without parameters.
	/// </summary>
	extension(IVoidMethodSetupReturnBuilder setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IVoidMethodSetup OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning void without parameters.
	/// </summary>
	extension(IVoidMethodSetupCallbackBuilder setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning void with one parameter.
	/// </summary>
	extension<T1>(IVoidMethodSetupReturnBuilder<T1> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IVoidMethodSetup<T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning void with one parameter.
	/// </summary>
	extension<T1>(IVoidMethodSetupCallbackBuilder<T1> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup<T1> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning void with two parameters.
	/// </summary>
	extension<T1, T2>(IVoidMethodSetupReturnBuilder<T1, T2> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning void with two parameters.
	/// </summary>
	extension<T1, T2>(IVoidMethodSetupCallbackBuilder<T1, T2> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning void with three parameters.
	/// </summary>
	extension<T1, T2, T3>(IVoidMethodSetupReturnBuilder<T1, T2, T3> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning void with three parameters.
	/// </summary>
	extension<T1, T2, T3>(IVoidMethodSetupCallbackBuilder<T1, T2, T3> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2, T3> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method setup returning void with four parameters.
	/// </summary>
	extension<T1, T2, T3, T4>(IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);

		/// <summary>
		///     Uses the return value only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}

	/// <summary>
	///     Extensions for method callback setup returning void with four parameters.
	/// </summary>
	extension<T1, T2, T3, T4>(IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}
}
