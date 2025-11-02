using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Protected;

public sealed partial class MockSetupsTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task GetEventHandlers_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new ProtectedMockSetup<int>(mock.Setup);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			@protected.AddEvent("foo.bar", setup, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(2);

			await That(@protected.GetEventHandlers("foo.bar")).HasCount(2);
		}

		[Fact]
		public async Task RegisterMethod_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup @protected = new ProtectedMockSetup<int>(mock.Setup);

			@protected.RegisterMethod(new ReturnMethodSetup<int>("foo.bar").Returns(42));

			int result = ((IMock)mock).Execute<int>("foo.bar").Result;
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task RegisterProperty_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup @protected = new ProtectedMockSetup<int>(mock.Setup);

			@protected.RegisterProperty("foo.bar", new PropertySetup<int>().InitializeWith(42));

			int result = ((IMock)mock).Get<int>("foo.bar");
			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task SubscribeToEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new ProtectedMockSetup<int>(mock.Setup);

			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
		}

		[Fact]
		public async Task UnsubscribeFromEvent_ShouldForwardToInner()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>();
			IMockSetup setup = mock.Setup;
			IMockSetup @protected = new ProtectedMockSetup<int>(mock.Setup);
			@protected.AddEvent("foo.bar", this, Helper.GetMethodInfo());

			await That(setup.GetEventHandlers("foo.bar")).HasCount(1);
			@protected.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());
			await That(setup.GetEventHandlers("foo.bar")).HasCount(0);
		}
	}
}
