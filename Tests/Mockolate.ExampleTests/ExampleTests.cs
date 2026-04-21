using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Mockolate.ExampleTests.TestData;
using Mockolate.Verify;
#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using Mockolate.Web;
#endif

namespace Mockolate.ExampleTests;

public class ExampleTests
{
	[Fact]
	public async Task Any_ShouldAlwaysMatch()
	{
		Guid id = Guid.NewGuid();
		MyClass sut = MyClass.CreateMock([3,])
			.Implementing<IExampleRepository>(setup => setup.AddUser(
					It.IsAny<string>())
				.Returns(new User(id, "Alice")))
			.Implementing<IOrderRepository>(setup => setup.AddOrder(
					It.IsAny<string>())
				.Returns(new Order(id, "Foo")));

		User result1 = ((IExampleRepository)sut).AddUser("Bob");
		Order result2 = ((IOrderRepository)sut).AddOrder("Bar");

		await That(result1).IsEqualTo(new User(id, "Alice"));
		await That(result2).IsEqualTo(new Order(id, "Foo"));
		sut.Mock.As<IExampleRepository>().Verify.AddUser(It.Is("Bob")).Once();
		sut.Mock.As<IOrderRepository>().Verify.AddOrder(It.Is("Bar")).Once();
	}

	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		MyClass sut = MyClass.CreateMock(3);

		sut.Mock.Setup.MyMethod(It.IsAny<int>()).Returns(5);

		int result = sut.MyMethod(3);

		VerificationResult<Mock.IMockVerifyForMyClass> check = sut.Mock.Verify.MyMethod(It.IsAny<int>());
		await That(result).IsEqualTo(5);
		check.Once();
	}

#if NET8_0_OR_GREATER
	[Theory]
	[InlineData(HttpStatusCode.OK)]
	[InlineData(HttpStatusCode.NotFound)]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	public async Task HttpClientTest(HttpStatusCode statusCode)
	{
		HttpClient httpClient = HttpClient.CreateMock();
		httpClient.Mock.Setup.PostAsync(It.Matches("*example.com*"), It.IsHttpContent())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		HttpResponseMessage result = await httpClient.PostAsync("https://www.example.com", new StringContent(""));

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif

	[Fact]
	public async Task MockIFileSystem_ShouldWork()
	{
		IFileSystem sut = IFileSystem.CreateMock(MockBehavior.Default.SkippingBaseClass());
		sut.Mock.Setup.Path.Returns(IPath.CreateMock());

		sut.Path.Mock.Setup.DirectorySeparatorChar.Returns('a');

		char result = sut.Path.DirectorySeparatorChar;

		await That(result).IsEqualTo('a');
	}

	[Fact]
	public async Task SimpleInterfaceMock()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock().Implementing<IOrderRepository>();
		sut.Mock.Setup.AddUser(It.IsAny<string>())
			.Returns(new User(id, "Alice"));
		User result = sut.AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		sut.Mock.Verify.AddUser(It.IsAny<string>()).Once();
	}

	[Fact]
	public async Task WithAdditionalInterface_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock().Implementing<IOrderRepository>();
		sut.Mock.As<IOrderRepository>().Setup.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)sut).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		sut.Mock.As<IOrderRepository>().Verify.AddOrder(It.Is("foo")).Once();
		await That(sut).Is<IExampleRepository>();
		await That(sut).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithAdditionalInterfaceInOtherOrder_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IOrderRepository sut = IOrderRepository.CreateMock().Implementing<IExampleRepository>();
		sut.Mock.As<IExampleRepository>().Setup.AddUser(It.IsAny<string>())
			.Returns(new User(id, "Alice"));
		User result = ((IExampleRepository)sut).AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		sut.Mock.As<IExampleRepository>().Verify.AddUser(It.IsAny<string>()).Once();
	}

	[Fact]
	public async Task WithEvent_ShouldSupportRaisingEvent()
	{
		EventArgs eventArgs = EventArgs.Empty;
		int raiseCount = 0;
		IExampleRepository sut = IExampleRepository.CreateMock();

		sut.Mock.Raise.UsersChanged(this, eventArgs);
		sut.UsersChanged += Register;
		sut.Mock.Raise.UsersChanged(this, eventArgs);
		sut.Mock.Raise.UsersChanged(this, eventArgs);
		sut.UsersChanged -= Register;
		sut.Mock.Raise.UsersChanged(this, eventArgs);

		await That(raiseCount).IsEqualTo(2);

		void Register(object? sender, EventArgs e)
		{
			if (sender == this && e == eventArgs)
			{
				raiseCount++;
			}
		}
	}

	[Fact]
	public async Task WithExplicitCastToAdditionalInterfaceSetup_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock().Implementing<IOrderRepository>();
		((IOrderRepository)sut).Mock.Setup.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)sut).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		sut.Mock.As<IOrderRepository>().Verify.AddOrder(It.Is("foo")).Once();
		await That(sut).Is<IExampleRepository>();
		await That(sut).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithExplicitCastToAdditionalInterfaceVerify_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock().Implementing<IOrderRepository>();
		sut.Mock.As<IOrderRepository>().Setup
			.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)sut).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		((IOrderRepository)sut).Mock.Verify.AddOrder(It.Is("foo")).Once();
		await That(sut).Is<IExampleRepository>();
		await That(sut).Is<IOrderRepository>();
	}

	[Theory]
	[InlineData("Alice", true)]
	[InlineData("Bob", false)]
	public async Task WithMatching_ShouldAlwaysMatch(string name, bool expectResult)
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock();

		sut.Mock.Setup.AddUser(
				It.Satisfies<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		User result = sut.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		sut.Mock.Verify.AddUser(It.Is(name)).Once();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		Guid id = Guid.NewGuid();
		IExampleRepository sut = IExampleRepository.CreateMock();

		sut.Mock.Setup.TryDelete(
				It.IsAny<Guid>(),
				It.IsOut<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		bool result = sut.TryDelete(id, out User? deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		sut.Mock.Verify.TryDelete(It.Is(id), It.IsOut<User?>()).Once();
	}
}
