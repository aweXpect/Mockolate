using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			var mock = Mock.Create<IMyService>();
			IMockSetup @protected = new MockSetups<int>.Protected(mock.Setup);

			@protected.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			var result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			var mock = Mock.Create<IMyService>();
			IMockSetup @protected = new MockSetups<int>.Protected(mock.Setup);

			@protected.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			var result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			var mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new MockSetups<int>.Protected(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			var mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new MockSetups<int>.Protected(mock.Setup);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			@protected.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(@protected.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			var mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new MockSetups<int>.Protected(mock.Setup);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			@protected.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}
}
