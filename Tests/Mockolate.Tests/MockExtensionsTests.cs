namespace Mockolate.Tests;

/* TODO
public sealed class MockExtensionsTests
{
	[Fact]
	public async Task ImplicitSetup_With1Type_ShouldSetupAndReturnTheMock()
	{
		IMyMockExtensionsService mock = Mock.Create<IMyMockExtensionsService>();

		IMyMockExtensionsService result = mock.SetupMock(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.DoSomething1()).IsEqualTo(3);
		await That(mock.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With2Types_ShouldSetupAndReturnTheMock()
	{
		IMyMockExtensionsService mock =
			Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2>();

		IMyMockExtensionsService result = mock.SetupMock(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.DoSomething1()).IsEqualTo(3);
		await That(mock.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With3Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3> mock =
			Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3> result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With4Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4>
			mock = Mock
				.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
					IMyMockExtensionsService4>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4>
			result = mock.Setup(
				m => m.Method.DoSomething1().Returns(3),
				m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With5Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5> mock =
			Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
				IMyMockExtensionsService4, IMyMockExtensionsService5>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5> result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With6Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6> mock =
			Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
				IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6> result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With7Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7> mock =
			Mock.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
				IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6,
				IMyMockExtensionsService7>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7> result = mock.Setup(
			m => m.Method.DoSomething1().Returns(3),
			m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With8Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
				IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7,
				IMyMockExtensionsService8>
			mock = Mock
				.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
					IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6,
					IMyMockExtensionsService7, IMyMockExtensionsService8>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
				IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7,
				IMyMockExtensionsService8>
			result = mock.Setup(
				m => m.Method.DoSomething1().Returns(3),
				m => m.Method.DoSomething2().Returns(5));

		await That(result).IsSameAs(mock);
		await That(mock.Subject.DoSomething1()).IsEqualTo(3);
		await That(mock.Subject.DoSomething2()).IsEqualTo(5);
	}

	[Fact]
	public async Task ImplicitSetup_With9Types_ShouldSetupAndReturnTheMock()
	{
		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7, IMyMockExtensionsService8,
			IMyMockExtensionsService9> mock = Mock
			.Create<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3,
				IMyMockExtensionsService4, IMyMockExtensionsService5, IMyMockExtensionsService6,
				IMyMockExtensionsService7, IMyMockExtensionsService8, IMyMockExtensionsService9>();

		Mock<IMyMockExtensionsService, IMyMockExtensionsService2, IMyMockExtensionsService3, IMyMockExtensionsService4,
			IMyMockExtensionsService5, IMyMockExtensionsService6, IMyMockExtensionsService7, IMyMockExtensionsService8,
			IMyMockExtensionsService9> result = mock.Setup(
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
*/
