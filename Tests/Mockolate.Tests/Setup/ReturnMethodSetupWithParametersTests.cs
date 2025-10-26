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
			.Returns("foo");

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.Any<int>(), With.Any<int>(), With.Any<int>())
			.Callback(() => { callCount++; })
			.Returns("foo");

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(With.AnyParameterCombination())).Once();
	}

	[Fact]
	public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; })
			.Returns((string?)null!);

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsNull();
	}

	public interface IReturnMethodSetupWithParametersTest
	{
		string MethodWithMultipleOverloads(int p1, int p2);
		string MethodWithMultipleOverloads(int p1, bool p2);
		string MethodWithOutParameter(int p1, out int p2);
		string MethodWithRefParameter(int p1, ref int p2);
		string MethodWithoutOtherOverloads(int p1, int p2, int p3);
		string MethodWithSingleParameter(int p1);
	}
}
