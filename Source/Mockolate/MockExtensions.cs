using System;
using System.Collections.Generic;
using System.Text;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///      Extensions for mocks.
/// </summary>
public static class MockExtensions
{
	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T> Setup<T>(this Mock<T> mock,
		params Action<MockSetup<T>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2> Setup<T1, T2>(this Mock<T1, T2> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3> Setup<T1, T2, T3>(this Mock<T1, T2, T3> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4> Setup<T1, T2, T3, T4>(this Mock<T1, T2, T3, T4> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4, T5> Setup<T1, T2, T3, T4, T5>(this Mock<T1, T2, T3, T4, T5> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4, T5, T6> Setup<T1, T2, T3, T4, T5, T6>(this Mock<T1, T2, T3, T4, T5, T6> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4, T5, T6, T7> Setup<T1, T2, T3, T4, T5, T6, T7>(this Mock<T1, T2, T3, T4, T5, T6, T7> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4, T5, T6, T7, T8> Setup<T1, T2, T3, T4, T5, T6, T7, T8>(this Mock<T1, T2, T3, T4, T5, T6, T7, T8> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}

	/// <summary>
	///     Sets up the <paramref name="mock" /> according to the given <paramref name="setups"/>.
	/// </summary>
	public static Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9> Setup<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9> mock,
		params Action<MockSetup<T1>>[] setups)
	{
		foreach (var setup in setups)
		{
			setup(mock.Setup);
		}
		return mock;
	}
}
