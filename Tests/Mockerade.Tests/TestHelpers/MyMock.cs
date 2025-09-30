using Mockerade.Checks;
using Mockerade.Events;
using Mockerade.Setup;

namespace Mockerade.Tests.TestHelpers;

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
