using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using System.Threading;
#endif
using Mockolate.Tests.Dummy;
using Mockolate.Verify;

namespace Mockolate.ExampleTests;

public class ExampleTests
{
	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		_ = Mock.Create<IList<int>>();
		Guid id = Guid.NewGuid();
		Mock<MyClass> mock = Mock.Create<MyClass>(BaseClass.WithConstructorParameters(3));

		mock.Setup.Method.MyMethod(With.Any<int>()).Returns(5);

		int result = mock.Subject.MyMethod(3);

		var check = mock.Verify.Invoked.MyMethod(With.Any<int>());
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
		Mock<HttpMessageHandler> mock = Mock.Create<HttpMessageHandler>();
		mock.Protected.Setup.Method
			.SendAsync(With.Any<HttpRequestMessage>(), With.Any<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		HttpClient httpClient = new(mock.Subject);

		HttpResponseMessage result = await httpClient.GetAsync("https://www.example.com");

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif
	[Fact]
	public async Task SimpleInterfaceMock()
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository, IOrderRepository> mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.Setup.Method
			.AddUser(With.Any<string>())
			.Returns(new User(id, "Alice"));
		User result = mock.Subject.AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.Verify.Invoked.AddUser(With.Any<string>()).Once();
	}

	[Fact]
	public async Task WithAdditionalInterface_ShouldWork()
	{
		EventArgs eventArgs = EventArgs.Empty;
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository, IOrderRepository> mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepository.Method
			.AddOrder(With.Any<string>())
			.Returns(new Order(id, "Order1"));

		var result = mock.SubjectForIOrderRepository.AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		mock.VerifyOnIOrderRepository.Invoked.AddOrder("foo").Once();
		await That(mock.Subject).Is<IExampleRepository>();
		await That(mock.Subject).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithAny_ShouldAlwaysMatch()
	{
		Guid id = Guid.NewGuid();
		Mock<MyClass, IExampleRepository, IOrderRepository> mock =
			Mock.Create<MyClass, IExampleRepository, IOrderRepository>(BaseClass.WithConstructorParameters(3));
		mock.SetupIExampleRepository.Method.AddUser(
				With.Any<string>())
			.Returns(new User(id, "Alice"));

		var result = mock.SubjectForIExampleRepository.AddUser("Bob");

		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.VerifyOnIExampleRepository.Invoked.AddUser("Bob").Once();
	}

	[Fact]
	public async Task WithEvent_ShouldSupportRaisingEvent()
	{
		EventArgs eventArgs = EventArgs.Empty;
		int raiseCount = 0;
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.Create<IExampleRepository>();

		mock.Raise.UsersChanged(this, eventArgs);
		mock.Subject.UsersChanged += Register;
		mock.Raise.UsersChanged(this, eventArgs);
		mock.Raise.UsersChanged(this, eventArgs);
		mock.Subject.UsersChanged -= Register;
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

	[Theory]
	[InlineData("Alice", true)]
	[InlineData("Bob", false)]
	public async Task WithMatching_ShouldAlwaysMatch(string name, bool expectResult)
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.Create<IExampleRepository>();

		mock.Setup.Method.AddUser(
				With.Matching<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		User result = mock.Subject.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		mock.Verify.Invoked.AddUser(name).Once();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.Create<IExampleRepository>();

		mock.Setup.Method.TryDelete(
				With.Any<Guid>(),
				With.Out<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		bool result = mock.Subject.TryDelete(id, out User? deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		mock.Verify.Invoked.TryDelete(id, With.Out<User?>()).Once();
	}

	[Fact]
	public async Task XXX()
	{
		bool isCalled = false;
		Guid id = Guid.NewGuid();
		Mock<IList<int>, IExampleRepository, IOrderRepository> mock = Mock.Create<IList<int>, IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepository.Method
			.SaveChanges()
			.Callback(() => isCalled = true);

		mock.SubjectForIExampleRepository.SaveChanges();

		await That(isCalled).IsFalse();
	}
}
