using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Internals;

public sealed class MockTests
{
	/* TODO
	[Fact]
	public async Task TryCast_WhenMatching_ShouldReturnTrue()
	{
		MyMock<MyServiceBase> mock = new(new MyServiceBase());
		object parameter = 42;

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(42);
	}

	[Fact]
	public async Task TryCast_WhenNotMatching_ShouldReturnFalse()
	{
		MyMock<MyServiceBase> mock = new(new MyServiceBase());
		object parameter = "foo";

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsFalse();
		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task TryCast_WhenNullShouldReturnTrue()
	{
		MyMock<MyServiceBase> mock = new(new MyServiceBase());
		object? parameter = null;

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(0);
	}
*/
	[Fact]
	public async Task WithTwoGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<IMyService, MyServiceBase>();

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase
			             """);
	}
	
	[Fact]
	public async Task WithThreeGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<IMyService, MyServiceBase, IChocolateDispenser, MyOtherServiceBase>();
	
		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The mock declaration has 2 additional implementations that are not interfaces: Mockolate.Tests.TestHelpers.MyServiceBase, Mockolate.Tests.Internals.MockTests.MyOtherServiceBase
			             """);
	}

	internal class MyOtherServiceBase;
}
