using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using static Mockolate.BaseClass;

namespace Mockolate.Tests;

public sealed partial class MockExtensionsTests
{
	[Fact]
	public async Task ImplicitSetup_With1Type_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);

	}

	[Fact]
	public async Task ImplicitSetup_With2Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With3Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With4Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With5Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With6Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With7Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With8Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With9Types_ShouldSetupAndReturnTheMock()
	{
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService, IMyMockExtensionsService>();

		var result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	public interface IMyMockExtensionsService
	{
		int DoSomething1();
		int DoSomething2();
	}
}
