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
		public async Task GenericMethods_ShouldConsiderGenericParameter()
		{
			Mock<IMethodService> mock = Mock.Create<IMethodService>();
			mock.Setup.Method.MyGenericMethod<int>().Returns(42);

			int matchingResult = mock.Subject.MyGenericMethod<int>();
			int notMatchingResult = mock.Subject.MyGenericMethod<long>();

			await That(matchingResult).IsEqualTo(42);
			await That(notMatchingResult).IsEqualTo(0);
		}

		[Fact]
		public async Task WhenNotSetup_ShouldReturnDefault()
		{
			Mock<IMethodService> mock = Mock.Create<IMethodService>();
			IMock sut = mock;

			MethodSetupResult<string> result0 = sut.Execute<string>("my.method");

			await That(result0.Result).IsEmpty();
		}

		[Fact]
		public async Task GenericMethod_SetupShouldWork()
		{
			Mock<IMethodService> mock = Mock.Create<IMethodService>();
			mock.Setup.Method.MyGenericMethod<int, string>(0, "foo").Returns(42);

			int result1 = mock.Subject.MyGenericMethod<int, string>(0, "foo");
			int result2 = mock.Subject.MyGenericMethod<long, string>(0, "foo");

			await That(mock.Verify.Invoked.MyGenericMethod<long, string>(With.Any<long>(), With.Any<string>())).Once();
			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(0);
		}

		public interface IMethodService
		{
			void MyVoidMethodWithoutParameters();
			void MyVoidMethodWithParameters(int x, string y);
			int MyIntMethodWithoutParameters();
			int MyIntMethodWithParameters(int x, string y);
			int MyGenericMethod<T1, T2>(T1 x, T2 y) where T1 : struct where T2 : class;
			int MyGenericMethod<T>();
		}
	}
}
