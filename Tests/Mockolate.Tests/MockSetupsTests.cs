using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockSetupsTests
{
	[Theory]
	[InlineData(0, 0, 0, 0, "(none)")]
	[InlineData(1, 0, 0, 0, "1 method")]
	[InlineData(2, 0, 0, 0, "2 methods")]
	[InlineData(0, 1, 0, 0, "1 property")]
	[InlineData(0, 2, 0, 0, "2 properties")]
	[InlineData(0, 0, 1, 0, "1 event")]
	[InlineData(0, 0, 2, 0, "2 events")]
	[InlineData(0, 0, 0, 1, "1 indexer")]
	[InlineData(0, 0, 0, 2, "2 indexers")]
	[InlineData(3, 5, 0, 2, "3 methods, 5 properties, 2 indexers")]
	[InlineData(3, 5, 8, 2, "3 methods, 5 properties, 8 events, 2 indexers")]
	public async Task ToString_Empty_ShouldReturnExpectedValue(
		int methodCount, int propertyCount, int eventCount, int indexerCount, string expected)
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		IMockSetup sut = mock.Setup;

		for (int i = 0; i < methodCount; i++)
		{
			sut.RegisterMethod(new ReturnMethodSetup<int>($"my.method{i}"));
		}

		for (int i = 0; i < propertyCount; i++)
		{
			sut.RegisterProperty($"my.property{i}", new PropertySetup<int>());
		}

		for (int i = 0; i < eventCount; i++)
		{
			sut.AddEvent($"my.event{i}", this, Helper.GetMethodInfo());
		}

		for (int i = 0; i < indexerCount; i++)
		{
			sut.RegisterIndexer(new IndexerSetup<string, int>(With.Any<int>()));
		}

		string result = mock.Setup.ToString();

		await That(result).IsEqualTo(expected);
	}

	public sealed class MethodsTests
	{
		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			proxy.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(proxy.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task Mock_ShouldBeInnerMock()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			await That(proxy.Mock).IsSameAs(setup.Mock);
		}

		[Fact]
		public async Task RegisterIndexer_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			proxy.RegisterIndexer(new IndexerSetup<int, string>("foo.bar").Returns(42));

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			proxy.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			int result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			proxy.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			int result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SetIndexerValue_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			proxy.SetIndexerValue(["foo.bar",], 42);

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Methods(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			proxy.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}

	public sealed class PropertiesTests
	{
		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			proxy.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(proxy.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task Mock_ShouldBeInnerMock()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			await That(proxy.Mock).IsSameAs(setup.Mock);
		}

		[Fact]
		public async Task RegisterIndexer_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			proxy.RegisterIndexer(new IndexerSetup<int, string>("foo.bar").Returns(42));

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			proxy.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			int result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			proxy.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			int result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SetIndexerValue_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			proxy.SetIndexerValue(["foo.bar",], 42);

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.Properties(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			proxy.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}

	public sealed class ProtectedMethodsTests
	{
		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			proxy.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(proxy.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task Mock_ShouldBeInnerMock()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			await That(proxy.Mock).IsSameAs(setup.Mock);
		}

		[Fact]
		public async Task RegisterIndexer_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			proxy.RegisterIndexer(new IndexerSetup<int, string>("foo.bar").Returns(42));

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			proxy.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			int result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			proxy.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			int result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SetIndexerValue_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			proxy.SetIndexerValue(["foo.bar",], 42);

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedMethods(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			proxy.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}

	public sealed class ProtectedPropertiesTests
	{
		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			proxy.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(proxy.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task Mock_ShouldBeInnerMock()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			await That(proxy.Mock).IsSameAs(setup.Mock);
		}

		[Fact]
		public async Task RegisterIndexer_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			proxy.RegisterIndexer(new IndexerSetup<int, string>("foo.bar").Returns(42));

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			proxy.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			int result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			proxy.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			int result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SetIndexerValue_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			proxy.SetIndexerValue(["foo.bar",], 42);

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<IMyService>.ProtectedProperties(mock.Setup);
			proxy.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			proxy.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}

	public sealed class ProxyTests
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
		public async Task Mock_ShouldBeInnerMock()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

			await That(proxy.Mock).IsSameAs(setup.Mock);
		}

		[Fact]
		public async Task RegisterIndexer_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

			proxy.RegisterIndexer(new IndexerSetup<int, string>("foo.bar").Returns(42));

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
			await That(result).IsEqualTo(42);
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
		public async Task SetIndexerValue_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup proxy = new MockSetup<int>.Proxy(mock.Setup);

			proxy.SetIndexerValue(["foo.bar",], 42);

			int result = ((IMock)mock).GetIndexer<int>(null, "foo.bar");
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
}
