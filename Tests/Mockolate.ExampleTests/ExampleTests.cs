using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Setup;
using Mockolate.Tests.Dummy;

namespace Mockolate.ExampleTests;


#nullable enable
public static class ForIExampleRepository_IOrderRepository
{
	/// <summary>
	///     The mock class for <see cref="IExampleRepository" /> and <see cref="IOrderRepository" />.
	/// </summary>
	public class Mock : Mock<IExampleRepository, IOrderRepository>
	{
		/// <inheritdoc cref="Mock" />
		public Mock(BaseClass.ConstructorParameters? constructorParameters, MockBehavior mockBehavior) : base(mockBehavior)
		{
			Object = Create<MockObject>(constructorParameters);
		}

		/// <inheritdoc cref="Mock{IExampleRepositoryIOrderRepository}.Object" />
		public override IExampleRepository Object { get; }
	}

	/// <summary>
	///     The actual mock object implementing <see cref="IExampleRepository" /> and <see cref="IOrderRepository" />.
	/// </summary>
	public partial class MockObject : IExampleRepository,
		IOrderRepository
	{
		private IMock _mock;

		/// <inheritdoc cref="MockObject" />
		public MockObject(IMock mock)
		{
			_mock = mock;
		}

		#region IExampleRepository
		/// <inheritdoc cref="IExampleRepository.UsersChanged" />
		public event EventHandler? UsersChanged
		{
			add => _mock.Raise.AddEvent("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", value?.Target, value?.Method);
			remove => _mock.Raise.RemoveEvent("Mockolate.Tests.Dummy.IExampleRepository.UsersChanged", value?.Target, value?.Method);
		}

		/// <inheritdoc cref="IExampleRepository.MyEvent" />
		public event MyDelegate? MyEvent
		{
			add => _mock.Raise.AddEvent("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", value?.Target, value?.Method);
			remove => _mock.Raise.RemoveEvent("Mockolate.Tests.Dummy.IExampleRepository.MyEvent", value?.Target, value?.Method);
		}

		/// <inheritdoc cref="IExampleRepository.AddUser(string)" />
		public User AddUser(string name)
		{
			var result = _mock.Execute<User>("Mockolate.Tests.Dummy.IExampleRepository.AddUser", name);
			return result.Result;
		}

		/// <inheritdoc cref="IExampleRepository.RemoveUser(Guid)" />
		public bool RemoveUser(Guid id)
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.RemoveUser", id);
			return result.Result;
		}

		/// <inheritdoc cref="IExampleRepository.UpdateUser(Guid, string)" />
		public void UpdateUser(Guid id, string newName)
		{
			var result = _mock.Execute("Mockolate.Tests.Dummy.IExampleRepository.UpdateUser", id, newName);
		}

		/// <inheritdoc cref="IExampleRepository.TryDelete(Guid, out User?)" />
		public bool TryDelete(Guid id, out User? user)
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.TryDelete", id, null);
			user = result.SetOutParameter<User?>("user");
			return result.Result;
		}

		/// <inheritdoc cref="IExampleRepository.SaveChanges()" />
		public bool SaveChanges()
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IExampleRepository.SaveChanges");
			return result.Result;
		}
		#endregion IExampleRepository

		#region IOrderRepository
		/// <inheritdoc cref="IOrderRepository.OrdersChanged" />
		event EventHandler? IOrderRepository.OrdersChanged
		{
			add => _mock.Raise.AddEvent("Mockolate.Tests.Dummy.IOrderRepository.OrdersChanged", value?.Target, value?.Method);
			remove => _mock.Raise.RemoveEvent("Mockolate.Tests.Dummy.IOrderRepository.OrdersChanged", value?.Target, value?.Method);
		}

		/// <inheritdoc cref="IOrderRepository.AddOrder(string)" />
		Order IOrderRepository.AddOrder(string name)
		{
			var result = _mock.Execute<Order>("Mockolate.Tests.Dummy.IOrderRepository.AddOrder", name);
			return result.Result;
		}

		/// <inheritdoc cref="IOrderRepository.RemoveOrder(Guid)" />
		bool IOrderRepository.RemoveOrder(Guid id)
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IOrderRepository.RemoveOrder", id);
			return result.Result;
		}

		/// <inheritdoc cref="IOrderRepository.UpdateOrder(Guid, string)" />
		void IOrderRepository.UpdateOrder(Guid id, string newName)
		{
			var result = _mock.Execute("Mockolate.Tests.Dummy.IOrderRepository.UpdateOrder", id, newName);
		}

		/// <inheritdoc cref="IOrderRepository.TryDelete(Guid, out Order?)" />
		bool IOrderRepository.TryDelete(Guid id, out Order? user)
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IOrderRepository.TryDelete", id, null);
			user = result.SetOutParameter<Order?>("user");
			return result.Result;
		}

		/// <inheritdoc cref="IOrderRepository.SaveChanges()" />
		bool IOrderRepository.SaveChanges()
		{
			var result = _mock.Execute<bool>("Mockolate.Tests.Dummy.IOrderRepository.SaveChanges");
			return result.Result;
		}
		#endregion IOrderRepository
	}

	extension(Mock<IExampleRepository, IOrderRepository> mock)
	{
		/// <summary>
		///     Sets up the mock for <see cref="IOrderRepository" />
		/// </summary>
		public MockSetups<IOrderRepository> SetupIOrderRepository
			=> new MockSetups<IOrderRepository>.Proxy(mock.Setup);

		/// <summary>
		///     Raise events on the mock for <see cref="IOrderRepository" />
		/// </summary>
		public MockRaises<IOrderRepository> RaiseOnIOrderRepository
			=> new MockRaises<IOrderRepository>(mock.Setup, ((IMock)mock).Checks);

		/// <summary>
		///     Check which methods got invoked on the mocked instance for <see cref="IOrderRepository" />
		/// </summary>
		public MockInvoked<IOrderRepository, Mock<IExampleRepository, IOrderRepository>> InvokedOnIOrderRepository
			=> new MockInvoked<IOrderRepository, Mock<IExampleRepository, IOrderRepository>>.Proxy(mock.Invoked, ((IMock)mock).Checks, mock);

		/// <summary>
		///     Check which properties were accessed on the mocked instance for <see cref="IOrderRepository" />
		/// </summary>
		public MockAccessed<IOrderRepository, Mock<IExampleRepository, IOrderRepository>> AccessedOnIOrderRepository
			=> new MockAccessed<IOrderRepository, Mock<IExampleRepository, IOrderRepository>>.Proxy(mock.Accessed, ((IMock)mock).Checks, mock);

		/// <summary>
		///     Exposes the mocked object instance of type <see cref="IOrderRepository" />
		/// </summary>
		public IOrderRepository ObjectForIOrderRepository
			=> (IOrderRepository)mock.Object;
	}
}


public class ExampleTests
{
	[Fact]
	public async Task SimpleInterfaceMock()
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository, IOrderRepository> mock = Mock.For<IExampleRepository, IOrderRepository>();
		mock.Setup
			.AddUser(With.Any<string>())
			.Returns(new User(id, "Alice"));
		User result = mock.Object.AddUser("Bob");
		await That(result).IsEqualTo(new User(id, "Alice"));
		await That(mock.Invoked.AddUser(With.Any<string>()).Once());
	}

	[Fact]
	public async Task BaseClassWithConstructorParameters()
	{
		Guid id = Guid.NewGuid();
		Mock<MyClass> mock = Mock.For<MyClass>(BaseClass.WithConstructorParameters(3));

		mock.Setup.MyMethod(With.Any<int>()).Returns(5);

		int result = mock.Object.MyMethod(3);

		await That(result).IsEqualTo(5);
		await That(mock.Invoked.MyMethod(With.Any<int>()).Once());
	}

#if NET8_0_OR_GREATER
	[Theory]
	[InlineData(HttpStatusCode.OK)]
	[InlineData(HttpStatusCode.NotFound)]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	public async Task HttpClientTest(HttpStatusCode statusCode)
	{
		Mock<HttpMessageHandler> mock = Mock.For<HttpMessageHandler>();
		mock.Protected.Setup
			.SendAsync(With.Any<HttpRequestMessage>(), With.Any<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage(statusCode));

		HttpClient httpClient = new(mock.Object);

		HttpResponseMessage result = await httpClient.GetAsync("https://www.example.com");

		await That(result.StatusCode).IsEqualTo(statusCode);
	}
#endif

	[Fact]
	public async Task WithAdditionalInterface_ShouldWork()
	{
		EventArgs eventArgs = EventArgs.Empty;
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository, IOrderRepository> mock = Mock.For<IExampleRepository, IOrderRepository>();
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
	public async Task WithAny_ShouldAlwaysMatch()
	{
		Guid id = Guid.NewGuid();
		Mock<MyClass, IExampleRepository, IOrderRepository> mock =
			Mock.For<MyClass, IExampleRepository, IOrderRepository>(BaseClass.WithConstructorParameters(3));
		mock.SetupIExampleRepository.AddUser(
				With.Any<string>())
			.Returns(new User(id, "Alice"));

		var result = mock.ObjectForIExampleRepository.AddUser("Bob");

		await That(result).IsEqualTo(new User(id, "Alice"));
		await That(mock.InvokedOnIExampleRepository.AddUser("Bob").Once());
	}

	[Fact]
	public async Task WithEvent_ShouldSupportRaisingEvent()
	{
		EventArgs eventArgs = EventArgs.Empty;
		int raiseCount = 0;
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.For<IExampleRepository>();

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

	[Theory]
	[InlineData("Alice", true)]
	[InlineData("Bob", false)]
	public async Task WithMatching_ShouldAlwaysMatch(string name, bool expectResult)
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.For<IExampleRepository>();

		mock.Setup.AddUser(
				With.Matching<string>(x => x == "Alice"))
			.Returns(new User(id, "Alice"));

		User result = mock.Object.AddUser(name);

		await That(result).IsEqualTo(expectResult ? new User(id, "Alice") : null);
		await That(mock.Invoked.AddUser(name).Once());
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task WithOut_ShouldSupportOutParameter(bool returnValue)
	{
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository> mock = Mock.For<IExampleRepository>();

		mock.Setup.TryDelete(
				With.Any<Guid>(),
				With.Out<User?>(() => new User(id, "Alice")))
			.Returns(returnValue);

		bool result = mock.Object.TryDelete(id, out User? deletedUser);

		await That(deletedUser).IsEqualTo(new User(id, "Alice"));
		await That(result).IsEqualTo(returnValue);
		await That(mock.Invoked.TryDelete(id, With.Out<User?>()).Once());
	}

	[Fact]
	public async Task XXX()
	{
		bool isCalled = false;
		Guid id = Guid.NewGuid();
		Mock<IExampleRepository, IOrderRepository> mock = Mock.For<IExampleRepository, IOrderRepository>();
		mock.SetupIOrderRepository
			.SaveChanges()
			.Callback(() => isCalled = true);

		bool result = mock.Object.SaveChanges();

		await That(isCalled).IsFalse();
	}
}
