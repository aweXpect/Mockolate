using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Verify;

namespace Mockolate;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public abstract class Mock<T> : MockBase<T>
{
	/// <inheritdoc cref="Mock{T}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T, Mock<T>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T" />.
	/// </summary>
	public MockVerify<T, Mock<T>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interface <typeparamref name="T2" />.
/// </summary>
public abstract class Mock<T1, T2> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" /> and <typeparamref name="T2" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" /> and
///     <typeparamref name="T3" />.
/// </summary>
public abstract class Mock<T1, T2, T3> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />
	///     and <typeparamref name="T3" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4>> Verify { get; }
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" /> and <typeparamref name="T5" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4, T5>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" />, <typeparamref name="T4" /> and <typeparamref name="T5" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4, T5>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" /> and <typeparamref name="T6" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" /> and <typeparamref name="T6" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" /> and
///     <typeparamref name="T7" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" /> and
	///     <typeparamref name="T7" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
///     <typeparamref name="T7" /> and <typeparamref name="T8" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7, T8}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		if (!typeof(T8).IsInterface)
		{
			throw new MockException($"The eighth generic type argument '{typeof(T8)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
	///     <typeparamref name="T7" /> and <typeparamref name="T8" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8>> Verify { get; }
}

/// <summary>
///     A mock for type <typeparamref name="T1" /> that also implements interfaces <typeparamref name="T2" />,
///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
///     <typeparamref name="T7" />, <typeparamref name="T8" /> and <typeparamref name="T9" />.
/// </summary>
public abstract class Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MockBase<T1>
{
	/// <inheritdoc cref="Mock{T1, T2, T3, T4, T5, T6, T7, T8, T9}" />
	protected Mock(MockBehavior behavior, string prefix) : base(behavior, prefix)
	{
		if (!typeof(T2).IsInterface)
		{
			throw new MockException($"The second generic type argument '{typeof(T2)}' is no interface.");
		}

		if (!typeof(T3).IsInterface)
		{
			throw new MockException($"The third generic type argument '{typeof(T3)}' is no interface.");
		}

		if (!typeof(T4).IsInterface)
		{
			throw new MockException($"The fourth generic type argument '{typeof(T4)}' is no interface.");
		}

		if (!typeof(T5).IsInterface)
		{
			throw new MockException($"The fifth generic type argument '{typeof(T5)}' is no interface.");
		}

		if (!typeof(T6).IsInterface)
		{
			throw new MockException($"The sixth generic type argument '{typeof(T6)}' is no interface.");
		}

		if (!typeof(T7).IsInterface)
		{
			throw new MockException($"The seventh generic type argument '{typeof(T7)}' is no interface.");
		}

		if (!typeof(T8).IsInterface)
		{
			throw new MockException($"The eighth generic type argument '{typeof(T8)}' is no interface.");
		}

		if (!typeof(T9).IsInterface)
		{
			throw new MockException($"The ninth generic type argument '{typeof(T9)}' is no interface.");
		}

		MockInteractions? checks = ((IMock)this).Interactions;
		Verify = new MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>>(checks, this);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T1" />, <typeparamref name="T2" />,
	///     <typeparamref name="T3" />, <typeparamref name="T4" />, <typeparamref name="T5" />, <typeparamref name="T6" />,
	///     <typeparamref name="T7" />, <typeparamref name="T8" /> and <typeparamref name="T9" />.
	/// </summary>
	public MockVerify<T1, Mock<T1, T2, T3, T4, T5, T6, T7, T8, T9>> Verify { get; }
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
