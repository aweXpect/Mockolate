using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Internals;

public sealed class MockTests
{
	[Fact]
	public async Task WithThreeGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<IMyService, MyServiceBase, IChocolateDispenser, MyOtherServiceBase>();
		}

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The mock declaration has 2 additional implementations that are not interfaces: Mockolate.Tests.TestHelpers.MyServiceBase, Mockolate.Tests.Internals.MockTests.MyOtherServiceBase
			             """);
	}

	[Fact]
	public async Task WithTwoGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<IMyService, MyServiceBase>();
		}

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase
			             """);
	}

	internal class MyOtherServiceBase;
}
