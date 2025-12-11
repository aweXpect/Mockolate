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
	extension<TValue, T1>(IIndexerSetupReturnWhenBuilder<TValue, T1> setup)
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
	extension<TValue, T1>(IIndexerSetupCallbackWhenBuilder<TValue, T1> setup)
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
	extension<TValue, T1, T2>(IIndexerSetupReturnWhenBuilder<TValue, T1, T2> setup)
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
	extension<TValue, T1, T2>(IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> setup)
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
	extension<TValue, T1, T2, T3>(IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> setup)
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
	extension<TValue, T1, T2, T3>(IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> setup)
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
	extension<TValue, T1, T2, T3, T4>(IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> setup)
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
	extension<TValue, T1, T2, T3, T4>(IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> setup)
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
	extension<TReturn>(IReturnMethodSetupReturnWhenBuilder<TReturn> setup)
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
	extension<TReturn>(IReturnMethodSetupCallbackWhenBuilder<TReturn> setup)
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
	extension<TReturn, T1>(IReturnMethodSetupReturnWhenBuilder<TReturn, T1> setup)
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
	extension<TReturn, T1>(IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> setup)
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
	extension<TReturn, T1, T2>(IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> setup)
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
	extension<TReturn, T1, T2>(IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> setup)
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
	extension<TReturn, T1, T2, T3>(IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> setup)
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
	extension<TReturn, T1, T2, T3>(IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> setup)
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
	extension<TReturn, T1, T2, T3, T4>(IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4> setup)
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
	extension<TReturn, T1, T2, T3, T4>(IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4> setup)
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
	extension(IVoidMethodSetupReturnWhenBuilder setup)
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
	extension(IVoidMethodSetupCallbackWhenBuilder setup)
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
	extension<T1>(IVoidMethodSetupReturnWhenBuilder<T1> setup)
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
	extension<T1>(IVoidMethodSetupCallbackWhenBuilder<T1> setup)
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
	extension<T1, T2>(IVoidMethodSetupReturnWhenBuilder<T1, T2> setup)
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
	extension<T1, T2>(IVoidMethodSetupCallbackWhenBuilder<T1, T2> setup)
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
	extension<T1, T2, T3>(IVoidMethodSetupReturnWhenBuilder<T1, T2, T3> setup)
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
	extension<T1, T2, T3>(IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> setup)
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
	extension<T1, T2, T3, T4>(IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4> setup)
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
	extension<T1, T2, T3, T4>(IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> setup)
	{
		/// <summary>
		///     Executes the callback only once.
		/// </summary>
		public IVoidMethodSetup<T1, T2, T3, T4> OnlyOnce()
			=> setup.Only(1);
	}
}
