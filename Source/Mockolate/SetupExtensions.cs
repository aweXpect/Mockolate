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
	}

	/// <summary>
	///     Extensions for indexer setups with two parameters.
	/// </summary>
	extension<TValue, T1>(IIndexerSetupReturnBuilder<TValue, T1> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);
	}

	/// <summary>
	///     Extensions for indexer setups with three parameters.
	/// </summary>
	extension<TValue, T1, T2>(IIndexerSetupReturnBuilder<TValue, T1, T2> setup)
	{
		/// <summary>
		///     Returns/throws forever.
		/// </summary>
		public void Forever()
			=> setup.For(int.MaxValue);
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
	}
}
