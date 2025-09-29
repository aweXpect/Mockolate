using Mockerade.Tests.Dummy;

namespace Mockerade.ExampleTests;

public class ExampleTests
{
	[Fact]
	public async Task WithAny_ShouldAlwaysMatch()
	{
		var id = Guid.NewGuid();
		var mock = Mock.For<IExampleRepository>();

		mock.Setup.AddUser(
				With.Any<string>())
			.Returns(new User(id, "Alice"));

		var result = mock.Object.AddUser("Bob");

		await That(result).IsEqualTo(new User(id, "Alice"));
		await That(mock.Invoked.AddUser("Bob").Once()).IsTrue();
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
		await That(mock.Invoked.AddUser(name).Once()).IsTrue();
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
		await That(mock.Invoked.TryDelete(id, With.Out<User?>()).Once()).IsTrue();
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
		await That(mock.InvokedOnIOrderRepository.AddOrder("foo").Once()).IsTrue();
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
