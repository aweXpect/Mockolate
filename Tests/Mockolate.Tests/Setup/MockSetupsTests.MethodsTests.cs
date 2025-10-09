using Mockolate.Setup;

namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class MethodsTests
	{
		[Fact]
		public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
		{
			Mock<IMethodService> mock = Mock.Create<IMethodService>();
			IMockSetup setup = mock.Setup;
			IMock sut = mock;

			MethodSetupResult<int> result0 = sut.Execute<int>("my.method");
			setup.RegisterMethod(new ReturnMethodSetup<int>("my.method").Returns(42));
			MethodSetupResult<int> result1 = sut.Execute<int>("my.method");

			await That(result0.Result).IsEqualTo(0);
			await That(result1.Result).IsEqualTo(42);
		}

		[Fact]
		public async Task WhenNotSetup_ShouldReturnDefault()
		{
			Mock<IMethodService> mock = Mock.Create<IMethodService>();
			IMock sut = mock;

			MethodSetupResult<string> result0 = sut.Execute<string>("my.method");

			await That(result0.Result).IsNull();
		}

		public interface IMethodService
		{
			void MyVoidMethodWithoutParameters();
			void MyVoidMethodWithParameters(int x, string y);
			int MyIntMethodWithoutParameters();
			int MyIntMethodWithParameters(int x, string y);
		}
	}
}
