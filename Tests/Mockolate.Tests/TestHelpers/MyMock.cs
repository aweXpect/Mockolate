using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Setup;

namespace Mockolate.Tests.TestHelpers;

public class MyMock<T>(T @object, MockBehavior? behavior = null) : Mock<T>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4, T5>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4, T5, T6>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4, T5, T6, T7>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7, T8>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4, T5, T6, T7, T8>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}

public class MyMock<T, T2, T3, T4, T5, T6, T7, T8, T9>(T @object = default!, MockBehavior? behavior = null) : Mock<T, T2, T3, T4, T5, T6, T7, T8, T9>(behavior ?? MockBehavior.Default)
{
	public IMockAccessed HiddenAccessed
		=> Accessed;

	public IMockEvent HiddenEvent
		=> Event;

	public IMockRaises HiddenRaise
		=> Raise;

	public IMockSetup HiddenSetup
		=> Setup;

	public IMock Hidden
		=> this;

	public override T Object => @object;
}
