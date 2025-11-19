namespace Mockolate.Tests.MockDelegates;

public class DelegateTests
{
	[Fact]
	public async Task Action_ShouldBeTreatedAsVoidDelegate()
	{
		bool isCalled = false;
		Action mock = Mock.Create<Action>();
		mock.SetupMock.Invoke().Do(() => isCalled = true);

		mock.Invoke();

		await That(mock.VerifyMock.Invoke()).Once();
		Assert.True(isCalled);
	}

	[Fact]
	public async Task Func_ShouldBeTreatedAsReturnDelegate()
	{
		bool isCalled = false;
		Func<int> mock = Mock.Create<Func<int>>();
		mock.SetupMock.Invoke().Do(() => isCalled = true).Returns(3);

		int result = mock();

		await That(mock.VerifyMock.Invoke()).Once();
		Assert.True(isCalled);
		Assert.Equal(3, result);
	}

	[Fact]
	public async Task WithCustomDelegate_SetupShouldWork()
	{
		DoSomething mock = Mock.Create<DoSomething>();
		mock.SetupMock.Invoke(Any<int>(), Any<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock(1, "foo");
		await That(() => mock(2, "foo")).Throws<Exception>().WithMessage("foobar");
		int result3 = mock(2, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Fact]
	public async Task WithCustomDelegate_VerifyShouldWork()
	{
		DoSomething mock = Mock.Create<DoSomething>();

		_ = mock(1, "foo");
		_ = mock(2, "bar");

		await That(mock.VerifyMock.Invoke(Any<int>(), Any<string>())).Twice();
	}

	[Fact]
	public async Task WithCustomDelegateWithRefAndOut_SetupShouldWork()
	{
		DoSomethingWithRefAndOut mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;
		mock.SetupMock.Invoke(Any<int>(), Ref<int>(v => v + 1), Out(() => 10));

		mock(1, ref value, out int value2);

		await That(mock.VerifyMock.Invoke(Any<int>(), Ref<int>(), Out<int>())).Once();
		await That(value).IsEqualTo(6);
		await That(value2).IsEqualTo(10);
	}

	[Fact]
	public async Task WithCustomDelegateWithRefAndOut_VerifyShouldWork()
	{
		DoSomethingWithRefAndOut mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;

		mock(1, ref value, out int _);

		await That(mock.VerifyMock.Invoke(Any<int>(), Ref<int>(), Out<int>())).Once();
		await That(value).IsEqualTo(5);
	}

	[Fact]
	public async Task WithCustomGenericDelegate_SetupShouldWork()
	{
		DoGeneric<long, string> mock = Mock.Create<DoGeneric<long, string>>();
		mock.SetupMock.Invoke(Any<long>(), Any<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock(1L, "foo");
		await That(() => mock(2L, "foo")).Throws<Exception>().WithMessage("foobar");
		int result3 = mock(2L, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Fact]
	public async Task WithCustomGenericDelegate_VerifyShouldWork()
	{
		DoGeneric<short, string> mock = Mock.Create<DoGeneric<short, string>>();

		_ = mock(1, "foo");
		_ = mock(2, "bar");

		await That(mock.VerifyMock.Invoke(Any<short>(), Any<string>())).Twice();
	}

	internal delegate int DoSomething(int x, string y);

	internal delegate void DoSomethingWithRefAndOut(int x, ref int y, out int z);

	internal delegate int DoGeneric<T1, T2>(T1 x, T2 y)
		where T1 : struct
		where T2 : class;
}
