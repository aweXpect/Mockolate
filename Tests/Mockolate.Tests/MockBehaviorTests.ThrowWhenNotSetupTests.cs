using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class ThrowWhenNotSetupTests
	{
		[Fact]
		public async Task WhenFalse_ShouldReturnDefaultValueInNotSetupMethods()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>(MockBehavior.Default with
			{
				ThrowWhenNotSetup = false,
			});

			string result = mock.Subject.DoSomethingAndReturn(5);

			await That(result).IsEqualTo("");
		}

		[Fact]
		public async Task WhenTrue_ShouldThrowMockNotSetupException()
		{
			Mock<IMyService> mock = Mock.Create<IMyService>(MockBehavior.Default with
			{
				ThrowWhenNotSetup = true,
			});

			void Act()
				=> mock.Subject.DoSomethingAndReturn(5);

			await That(Act).Throws<MockNotSetupException>()
				.WithMessage(
					"The method 'Mockolate.Tests.TestHelpers.IMyService.DoSomethingAndReturn(int)' was invoked without prior setup.");
		}
	}
}
