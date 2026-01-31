using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Mockolate.ExampleTests.TestData;
using Mockolate.Monitor;
using Mockolate.Verify;
#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Mockolate.Web;
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
				It.IsAny<string>())
			.Returns(new User(id, "Alice"));

		User result = ((IExampleRepository)mock).AddUser("Bob");

		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.VerifyOnIExampleRepositoryMock.Invoked.AddUser(It.Is("Bob")).Once();
	}

	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		MyClass mock = Mock.Create<MyClass>(BaseClass.WithConstructorParameters(3));

		mock.SetupMock.Method.MyMethod(It.IsAny<int>()).Returns(5);

		int result = mock.MyMethod(3);

		VerificationResult<MyClass> check = mock.VerifyMock.Invoked.MyMethod(It.IsAny<int>());
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
		HttpClient httpClient = Mock.Create<HttpClient>();
		httpClient.SetupMock.Method
			.PostAsync(It.Matches("*example.com*"), It.IsStringContent())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		HttpResponseMessage result = await httpClient.PostAsync("https://www.example.com", new StringContent(""));

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif

	[Fact]
	public async Task MockIFileSystem_ShouldWork()
	{
		IFileSystem mock = Mock.Create<IFileSystem>(MockBehavior.Default.SkippingBaseClass());
		mock.SetupMock.Property.Path.Returns(Mock.Create<IPath>());

		mock.Path.SetupMock.Property.DirectorySeparatorChar.Returns('a');

		char result = mock.Path.DirectorySeparatorChar;

		await That(result).IsEqualTo('a');
	}

	[Fact]
	public async Task SimpleInterfaceMock()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupMock.Method
			.AddUser(It.IsAny<string>())
			.Returns(new User(id, "Alice"));
		User result = mock.AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		mock.VerifyMock.Invoked.AddUser(It.IsAny<string>()).Once();
	}

	[Fact]
	public async Task WithAdditionalInterface_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepositoryMock.Method
			.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		mock.VerifyOnIOrderRepositoryMock.Invoked.AddOrder(It.Is("foo")).Once();
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
			.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		mock.VerifyOnIOrderRepositoryMock.Invoked.AddOrder(It.Is("foo")).Once();
		await That(mock).Is<IExampleRepository>();
		await That(mock).Is<IOrderRepository>();
	}

	[Fact]
	public async Task WithExplicitCastToAdditionalInterfaceVerify_ShouldWork()
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepositoryMock.Method
			.AddOrder(It.IsAny<string>())
			.Returns(new Order(id, "Order1"));

		Order result = ((IOrderRepository)mock).AddOrder("foo");

		await That(result.Name).IsEqualTo("Order1");
		((IOrderRepository)mock).VerifyMock.Invoked.AddOrder(It.Is("foo")).Once();
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
				It.Satisfies<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		User result = mock.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		mock.VerifyMock.Invoked.AddUser(It.Is(name)).Once();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		Guid id = Guid.NewGuid();
		IExampleRepository mock = Mock.Create<IExampleRepository>();

		mock.SetupMock.Method.TryDelete(
				It.IsAny<Guid>(),
				It.IsOut<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		bool result = mock.TryDelete(id, out User? deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		mock.VerifyMock.Invoked.TryDelete(It.Is(id), It.IsOut<User?>()).Once();
	}

	[Fact]
	public async Task InteractionTracking_ShouldTrackAllInteractions()
	{
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		IExampleRepository sut = Mock.Create<IExampleRepository>();

		sut.AddUser("Alice");
		sut.RemoveUser(id1);
		sut.AddUser("Bob");

		// Access all interactions via cast to access the concrete Mock type
		Mock<IExampleRepository> mockInstance = ((IMockSubject<IExampleRepository>)sut).Mock;
		await That(mockInstance.Interactions.Count).IsEqualTo(3);
		await That(mockInstance.Interactions.Interactions).HasCount(3);

		// Clear interaction history
		sut.SetupMock.ClearAllInteractions();
		await That(mockInstance.Interactions.Count).IsEqualTo(0);
	}

#if NET8_0_OR_GREATER
	[Fact]
	public async Task MockMonitoring_ShouldTrackScopedInteractions()
	{
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		IExampleRepository sut = Mock.Create<IExampleRepository>();
		sut.SetupMock.Method.AddUser(It.IsAny<string>()).Returns(name => new User(Guid.NewGuid(), name));

		sut.AddUser("Before");
		
		using (IDisposable monitor = sut.MonitorMock(out MockMonitor<IExampleRepository> monitorInstance))
		{
			sut.AddUser("Alice");
			sut.AddUser("Bob");

			// Verify interactions within this session only
			monitorInstance.Verify.Invoked.AddUser(It.Is("Alice")).Once();
			monitorInstance.Verify.Invoked.AddUser(It.Is("Bob")).Once();
			// "Before" was called outside the monitoring session
			monitorInstance.Verify.Invoked.AddUser(It.Is("Before")).Never();
		}

		sut.AddUser("After");

		// The main mock still tracks all interactions
		sut.VerifyMock.Invoked.AddUser(It.Is("Before")).Once();
		sut.VerifyMock.Invoked.AddUser(It.Is("Alice")).Once();
		sut.VerifyMock.Invoked.AddUser(It.Is("Bob")).Once();
		sut.VerifyMock.Invoked.AddUser(It.Is("After")).Once();
	}
#endif
}
