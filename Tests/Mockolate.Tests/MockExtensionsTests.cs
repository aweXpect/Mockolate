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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4, IMyMockExtensionsService5>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7, IMyMockExtensionsService8>();

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
		var mock = Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7, IMyMockExtensionsService8, IMyMockExtensionsService9>();

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

	public interface IMyMockExtensionsService2
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService3
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService4
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService5
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService6
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService7
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService8
	{
		int DoSomething();
	}

	public interface IMyMockExtensionsService9
	{
		int DoSomething();
	}
}
