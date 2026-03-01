namespace Mockolate.Tests.MockDelegates;

public class DelegateTests
{
	[Test]
	public async Task Action_ShouldBeTreatedAsVoidDelegate()
	{
		bool isCalled = false;
		Action mock = Mock.Create<Action>();
		mock.SetupMock.Delegate().Do(() => isCalled = true);

		mock.Invoke();

		await That(mock.VerifyMock.Invoked()).Once();
		await That(isCalled).IsTrue();
	}

	[Test]
	public async Task Func_ShouldBeTreatedAsReturnDelegate()
	{
		bool isCalled = false;
		Func<int> mock = Mock.Create<Func<int>>();
		mock.SetupMock.Delegate().Do(() => isCalled = true).Returns(3);

		int result = mock();

		await That(mock.VerifyMock.Invoked()).Once();
		await That(isCalled).IsTrue();
		await That(result).IsEqualTo(3);
	}

	[Test]
	public async Task WithCustomDelegate_SetupShouldWork()
	{
		DoSomething mock = Mock.Create<DoSomething>();
		mock.SetupMock.Delegate(It.IsAny<int>(), It.IsAny<string>(), It.IsTrue())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock(1, "foo", true);
		await That(() => mock(2, "foo", true)).Throws<Exception>().WithMessage("foobar");
		int result3 = mock(2, "bar", true);

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Test]
	public async Task WithCustomDelegate_VerifyShouldWork()
	{
		DoSomething mock = Mock.Create<DoSomething>();

		_ = mock(1, "foo", true);
		_ = mock(2, "bar", true);

		await That(mock.VerifyMock.Invoked(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Twice();
	}

	[Test]
	public async Task WithCustomDelegateWithRefAndOut_SetupShouldWork()
	{
		DoSomethingWithRefAndOut mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;
		mock.SetupMock.Delegate(It.IsAny<int>(), It.IsRef<int>(v => v + 1), It.IsOut(() => 10));

		mock(1, ref value, out int value2);

		await That(mock.VerifyMock.Invoked(It.IsAny<int>(), It.IsRef<int>(), It.IsOut<int>())).Once();
		await That(value).IsEqualTo(6);
		await That(value2).IsEqualTo(10);
	}

	[Test]
	public async Task WithCustomDelegateWithRefAndOut_VerifyShouldWork()
	{
		DoSomethingWithRefAndOut mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;

		mock(1, ref value, out int _);

		await That(mock.VerifyMock.Invoked(It.IsAny<int>(), It.IsRef<int>(), It.IsOut<int>())).Once();
		await That(value).IsEqualTo(5);
	}

	[Test]
	public async Task WithCustomGenericDelegate_SetupShouldWork()
	{
		DoGeneric<long, string> mock = Mock.Create<DoGeneric<long, string>>();
		mock.SetupMock.Delegate(It.IsAny<long>(), It.IsAny<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock(1L, "foo");
		await That(() => mock(2L, "foo")).Throws<Exception>().WithMessage("foobar");
		int result3 = mock(2L, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Test]
	public async Task WithCustomGenericDelegate_VerifyShouldWork()
	{
		DoGeneric<short, string> mock = Mock.Create<DoGeneric<short, string>>();

		_ = mock(1, "foo");
		_ = mock(2, "bar");

		await That(mock.VerifyMock.Invoked(It.IsAny<short>(), It.IsAny<string>())).Twice();
	}

	internal delegate int DoSomething(int x, string y, bool p);

	internal delegate void DoSomethingWithRefAndOut(int x, ref int y, out int z);

	internal delegate int DoGeneric<T1, T2>(T1 x, T2 y)
		where T1 : struct
		where T2 : class;
}
