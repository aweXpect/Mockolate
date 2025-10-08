using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Setup;
using static Mockolate.BaseClass;

namespace Mockolate.Tests.TestHelpers;

public class MyMock<T>(T @object, MockBehavior? behavior = null) : Mock<T>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;

	public bool HiddenTryCast<TValue>(object? value, out TValue result)
		=> TryCast(value, out result);
}

public class MyMock<T, T2>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4, T5>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4, T5>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4, T5>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4, T5, T6>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4, T5, T6>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4, T5, T6>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4, T5, T6, T7>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4, T5, T6, T7>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4, T5, T6, T7>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7, T8>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4, T5, T6, T7, T8>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4, T5, T6, T7, T8>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4, T5, T6, T7, T8>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7, T8, T9>(T @object = default!, MockBehavior? behavior = null)
	: Mock<T, T2, T3, T4, T5, T6, T7, T8, T9>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed<Mock<T, T2, T3, T4, T5, T6, T7, T8, T9>> HiddenAccessed
		=> Accessed;

	public IMockEvent<Mock<T, T2, T3, T4, T5, T6, T7, T8, T9>> HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}
