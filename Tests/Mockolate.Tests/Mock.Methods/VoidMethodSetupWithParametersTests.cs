namespace Mockolate.Tests.Setup;

public class VoidMethodSetupWithParametersTests
{
	[Fact]
	public async Task Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		Mock<IVoidMethodSetupWithParametersTest> sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; });

		sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		Mock<IVoidMethodSetupWithParametersTest> sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.Any<int>(), With.Any<int>(), With.Any<int>())
			.Callback(() => { callCount++; });

		sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(With.AnyParameterCombination())).Once();
	}

	public interface IVoidMethodSetupWithParametersTest
	{
		void MethodWithMultipleOverloads(int p1, int p2);
		void MethodWithMultipleOverloads(int p1, bool p2);
		void MethodWithOutParameter(int p1, out int p2);
		void MethodWithRefParameter(int p1, ref int p2);
		void MethodWithoutOtherOverloads(int p1, int p2, int p3);
		void MethodWithSingleParameter(int p1);
	}
}
