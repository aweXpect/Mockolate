using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Tests.Dummy;

namespace Mockolate.ExampleTests;

public class ExampleTests
{
	[Fact]
	public async Task WithAny_ShouldAlwaysMatch()
	{
		var id = Guid.NewGuid();
		var mock = Mock.For<MyClass, IExampleRepository, IOrderRepository>(BaseClass.WithConstructorParameters(3));
		mock.SetupIExampleRepository.AddUser(
				With.Any<string>())
			.Returns(new User(id, "Alice"));
		
		var result = mock.ObjectForIExampleRepository.AddUser("Bob");
		
		await That(result).IsEqualTo(new User(id, "Alice"));
		await That(mock.InvokedOnIExampleRepository.AddUser("Bob").Once());
	}

#if NET8_0_OR_GREATER
	[Theory]
	[InlineData(HttpStatusCode.OK)]
	[InlineData(HttpStatusCode.NotFound)]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	public async Task HttpClientTest(System.Net.HttpStatusCode statusCode)
	{
		var mock = Mock.For<HttpMessageHandler>();
		mock.Protected.Setup
			.SendAsync(With.Any<HttpRequestMessage>(), With.Any<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		var httpClient = new HttpClient(mock.Object);

		var result = await httpClient.GetAsync("https://www.example.com", TestContext.Current.CancellationToken);

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif

	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		var id = Guid.NewGuid();
		var mock = Mock.For<MyClass>(BaseClass.WithConstructorParameters(3));

		mock.Setup.MyMethod(With.Any<int>()).Returns(5);
		
		var result = mock.Object.MyMethod(3);

		await That(result).IsEqualTo(5);
		await That(mock.Invoked.MyMethod(With.Any<int>()).Once());
	}

	[Theory]
	[InlineData("Alice", true)]
	[InlineData("Bob", false)]
	public async Task WithMatching_ShouldAlwaysMatch(string name, bool expectResult)
	{
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository>();

		mock.Setup.AddUser(
				With.Matching<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		var result = mock.Object.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		await That(mock.Invoked.AddUser(name).Once());
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository>();

		mock.Setup.TryDelete(
				With.Any<Guid>(),
				With.Out<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		var result = mock.Object.TryDelete(id, out var deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		await That(mock.Invoked.TryDelete(id, With.Out<User?>()).Once());
	}

	[Fact]
	public async Task WithEvent_ShouldSupportRaisingEvent()
	{
		var eventArgs = EventArgs.Empty;
		int raiseCount = 0;
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository>();

		mock.Raise.UsersChanged(this, eventArgs);
		mock.Object.UsersChanged += Register;
		mock.Raise.UsersChanged(this, eventArgs);
		mock.Raise.UsersChanged(this, eventArgs);
		mock.Object.UsersChanged -= Register;
		mock.Raise.UsersChanged(this, eventArgs);

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
	public async Task WithAdditionalInterface_ShouldWork()
	{
		var eventArgs = EventArgs.Empty;
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepository
			.AddOrder(With.Any<string>())
			.Returns(new Order(id, "Order1"));

		var result = mock.ObjectForIOrderRepository.AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		await That(mock.InvokedOnIOrderRepository.AddOrder("foo").Once());
		await That(mock.Object).Is<IExampleRepository>();
		await That(mock.Object).Is<IOrderRepository>();
	}

	[Fact]
	public async Task XXX()
	{
		bool isCalled = false;
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepository
			.SaveChanges()
			.Callback(() => isCalled = true);

		var result = mock.Object.SaveChanges();

		await That(isCalled).IsFalse();
	}
}
