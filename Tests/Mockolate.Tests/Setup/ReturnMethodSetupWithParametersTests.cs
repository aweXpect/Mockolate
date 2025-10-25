using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Tests.Setup;

public class ReturnMethodSetupWithParametersTests
{
	[Fact]
	public async Task Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; })
			.Returns(42);

		int result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo(42);
	}

	[Fact]
	public async Task Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.Any<int>(), With.Any<int>(), With.Any<int>())
			.Callback(() => { callCount++; })
			.Returns(42);

		int result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo(42);
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(With.AnyParameterCombination())).Once();
	}

	public interface IReturnMethodSetupWithParametersTest
	{
		int MethodWithMultipleOverloads(int p1, int p2);
		int MethodWithMultipleOverloads(int p1, bool p2);
		int MethodWithOutParameter(int p1, out int p2);
		int MethodWithRefParameter(int p1, ref int p2);
		int MethodWithoutOtherOverloads(int p1, int p2, int p3);
		int MethodWithSingleParameter(int p1);
	}
}
