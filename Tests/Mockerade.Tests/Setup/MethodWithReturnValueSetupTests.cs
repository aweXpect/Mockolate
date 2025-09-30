using Mockerade.Setup;
using Mockerade.Tests.TestHelpers;

namespace Mockerade.Tests.Setup;

public class ReturnMethodSetupTests
{
	public sealed class WithoutParametersTests
	{
		[Theory]
		[InlineData("FooMethod", true)]
		[InlineData("BarMethod", false)]
		public async Task Callback_ForMatchingMethod_ShouldBeExecuted(string calledMethod, bool expectedResult)
		{
			var mock = new MyMock<int>(3);
			bool isCalled = false;
			var sut = new ReturnMethodSetup<int>("FooMethod");
			sut.Callback(() => { isCalled = true; });
			mock.HiddenSetup.RegisterMethod(sut);

			((IMock)mock).Execute<int>(calledMethod);

			await That(isCalled).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData("FooMethod", 42)]
		[InlineData("BarMethod", 0)]
		public async Task Returns_ForMatchingMethod_ShouldReturnExpectedValue(string calledMethod, int expectedValue)
		{
			var mock = new MyMock<int>(3);
			var sut = new ReturnMethodSetup<int>("FooMethod");
			sut.Returns(42);
			mock.HiddenSetup.RegisterMethod(sut);

			int result = ((IMock)mock).Execute<int>(calledMethod).Result;

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData("FooMethod", 42)]
		[InlineData("BarMethod", 0)]
		public async Task Returns_WithCallback_ForMatchingMethod_ShouldReturnExpectedValue(string calledMethod, int expectedValue)
		{
			var mock = new MyMock<int>(3);
			var sut = new ReturnMethodSetup<int>("FooMethod");
			sut.Returns(() => 42);
			mock.HiddenSetup.RegisterMethod(sut);

			int result = ((IMock)mock).Execute<int>(calledMethod).Result;

			await That(result).IsEqualTo(expectedValue);
		}
	}
	public sealed class With1ParameterTests
	{
		[Theory]
		[InlineData("FooMethod", true)]
		[InlineData("BarMethod", false)]
		public async Task Callback_ForMatchingMethod_ShouldBeExecuted(string calledMethod, bool expectedResult)
		{
			var mock = new MyMock<int>(3);
			bool isCalled = false;
			var sut = new ReturnMethodSetup<int, string>("FooMethod", new With.NamedParameter("x", With.Any<string>()));
			sut.Callback(() => { isCalled = true; });
			mock.HiddenSetup.RegisterMethod(sut);

			((IMock)mock).Execute<int>(calledMethod, "bar");

			await That(isCalled).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData("FooMethod", 42)]
		[InlineData("BarMethod", 0)]
		public async Task Returns_ForMatchingMethod_ShouldReturnExpectedValue(string calledMethod, int expectedValue)
		{
			var mock = new MyMock<int>(3);
			var sut = new ReturnMethodSetup<int, string>("FooMethod", new With.NamedParameter("x", With.Any<string>()));
			sut.Returns(42);
			mock.HiddenSetup.RegisterMethod(sut);

			int result = ((IMock)mock).Execute<int>(calledMethod, "bar").Result;

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData("FooMethod", 42)]
		[InlineData("BarMethod", 0)]
		public async Task Returns_WithCallback_ForMatchingMethod_ShouldReturnExpectedValue(string calledMethod, int expectedValue)
		{
			var mock = new MyMock<int>(3);
			var sut = new ReturnMethodSetup<int, string>("FooMethod", new With.NamedParameter("x", With.Any<string>()));
			sut.Returns(() => 42);
			mock.HiddenSetup.RegisterMethod(sut);

			int result = ((IMock)mock).Execute<int>(calledMethod, "bar").Result;

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task Returns_WithCallback_ShouldReceiveParameter()
		{
			string receivedParameter = string.Empty;
			var mock = new MyMock<int>(3);
			var sut = new ReturnMethodSetup<int, string>("FooMethod", new With.NamedParameter("x", With.Any<string>()));
			sut.Returns(p => {
				receivedParameter = p;
				return 42;
			});
			mock.HiddenSetup.RegisterMethod(sut);

			int result = ((IMock)mock).Execute<int>("FooMethod", "bar").Result;

			await That(result).IsEqualTo(42);
			await That(receivedParameter).IsEqualTo("bar");
		}
	}
}
