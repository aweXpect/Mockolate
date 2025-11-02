using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Setup;

public sealed class MockSetupsProxyTests
{
	[Fact]
	public async Task GetEventHandlers_ShouldForwardToInner()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup setup = mock.Setup;
		IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);
		proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
		proxy.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
		await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

		await That(proxy.GetEventHandlers("foo.bar")).HasCount(2);
	}

	[Fact]
	public async Task RegisterMethod_ShouldForwardToInner()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

		proxy.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

		int result = ((IMock)mock).Execute<int>("foo.bar").Result;
		await That(result).IsEqualTo(42);
	}

	[Fact]
	public async Task RegisterProperty_ShouldForwardToInner()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

		proxy.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

		int result = ((IMock)mock).Get<int>("foo.bar");
		await That(result).IsEqualTo(42);
	}

	[Fact]
	public async Task SubscribeToEvent_ShouldForwardToInner()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup setup = mock.Setup;
		IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

		await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
		await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
	}

	[Fact]
	public async Task UnsubscribeFromEvent_ShouldForwardToInner()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup setup = mock.Setup;
		IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);
		proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		proxy.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
		await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
	}
}
