using Mockolate.Tests.Dummy;
using Mockolate.Verify;
#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using System.Threading;
#endif

namespace Mockolate.ExampleTests;

public class ExampleTests
{
	[Fact]
	public async Task Any_ShouldAlwaysMatch()
	{
		Guid id = Guid.NewGuid();
		MyClass mock =
			Mock.Create<MyClass, IExampleRepository, IOrderRepository>(BaseClass.WithConstructorParameters(3));
		mock.SetupIExampleRepositoryMock.Method.AddUser(
				Any<string>())
			.Returns(new User(id, "Alice"));

		User result = ((IExampleRepository)mock).AddUser("Bob");

		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.VerifyOnIExampleRepositoryMock.Invoked.AddUser(With("Bob")).Once();
	}

	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		MyClass mock = Mock.Create<MyClass>(BaseClass.WithConstructorParameters(3));

		mock.SetupMock.Method.MyMethod(Any<int>()).Returns(5);

		int result = mock.MyMethod(3);

		VerificationResult<MyClass> check = mock.VerifyMock.Invoked.MyMethod(Any<int>());
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
		HttpMessageHandler httpMessageHandler = Mock.Create<HttpMessageHandler>();
		httpMessageHandler.SetupMock.Protected.Method
			.SendAsync(Any<HttpRequestMessage>(), Any<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		HttpClient httpClient = new(httpMessageHandler);

		HttpResponseMessage result = await httpClient.GetAsync("https://www.example.com");

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif
	[Fact]
	public async Task SimpleInterfaceMock()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupMock.Method
			.AddUser(Any<string>())
			.Returns(new User(id, "Alice"));
		User result = mock.AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.VerifyMock.Invoked.AddUser(Any<string>()).Once();
	}

	[Fact]
	public async Task WithAdditionalInterface_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepositoryMock.Method
			.AddOrder(Any<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		mock.VerifyOnIOrderRepositoryMock.Invoked.AddOrder(With("foo")).Once();
		await That(mock).Is<IExampleRepository>();
		await That(mock).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithEvent_ShouldSupportRaisingEvent()
	{
		EventArgs eventArgs = EventArgs.Empty;
		int raiseCount = 0;
		IExampleRepository mock = Mock.Create<IExampleRepository>();

		mock.RaiseOnMock.UsersChanged(this, eventArgs);
		mock.UsersChanged += Register;
		mock.RaiseOnMock.UsersChanged(this, eventArgs);
		mock.RaiseOnMock.UsersChanged(this, eventArgs);
		mock.UsersChanged -= Register;
		mock.RaiseOnMock.UsersChanged(this, eventArgs);

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
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		((IOrderRepository)mock).SetupMock.Method
			.AddOrder(Any<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		mock.VerifyOnIOrderRepositoryMock.Invoked.AddOrder(With("foo")).Once();
		await That(mock).Is<IExampleRepository>();
		await That(mock).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithExplicitCastToAdditionalInterfaceVerify_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepositoryMock.Method
			.AddOrder(Any<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		((IOrderRepository)mock).VerifyMock.Invoked.AddOrder(With("foo")).Once();
		await That(mock).Is<IExampleRepository>();
		await That(mock).Is<IOrderRepository>();
	}

	[Theory]
	[InlineData("Alice", true)]
	[InlineData("Bob", false)]
	public async Task WithMatching_ShouldAlwaysMatch(string name, bool expectResult)
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository>();

		mock.SetupMock.Method.AddUser(
				With<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		User result = mock.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		mock.VerifyMock.Invoked.AddUser(With(name)).Once();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository>();

		mock.SetupMock.Method.TryDelete(
				Any<Guid>(),
				Out<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		bool result = mock.TryDelete(id, out User? deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		mock.VerifyMock.Invoked.TryDelete(With(id), Out<User?>()).Once();
	}
}
