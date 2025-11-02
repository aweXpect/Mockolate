using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Internals;

public sealed class MockTests
{
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

	[Fact]
	public async Task WithTwoGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyServiceBase>();

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.
			             """);
	}
}
