namespace Mockolate.Tests.MockDelegates;

public class DelegateTests
{
	[Fact]
	public async Task Action_ShouldBeTreatedAsVoidDelegate()
	{
		bool isCalled = false;
		Mock<Action> mock = Mock.Create<Action>();
		mock.Setup.Delegate().Callback(() => isCalled = true);

		mock.Subject.Invoke();

		await That(mock.Verify.Invoked()).Once();
		Assert.True(isCalled);
	}

	[Fact]
	public async Task Func_ShouldBeTreatedAsReturnDelegate()
	{
		bool isCalled = false;
		Mock<Func<int>> mock = Mock.Create<Func<int>>();
		mock.Setup.Delegate().Callback(() => isCalled = true).Returns(3);

		int result = mock.Subject();

		await That(mock.Verify.Invoked()).Once();
		Assert.True(isCalled);
		Assert.Equal(3, result);
	}

	[Fact]
	public async Task WithCustomDelegate_SetupShouldWork()
	{
		Mock<DoSomething> mock = Mock.Create<DoSomething>();
		mock.Setup.Delegate(WithAny<int>(), WithAny<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock.Subject(1, "foo");
		await That(() => mock.Subject(2, "foo")).Throws<Exception>().WithMessage("foobar");
		int result3 = mock.Subject(2, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Fact]
	public async Task WithCustomDelegate_VerifyShouldWork()
	{
		Mock<DoSomething> mock = Mock.Create<DoSomething>();

		_ = mock.Subject(1, "foo");
		_ = mock.Subject(2, "bar");

		await That(mock.Verify.Invoked(WithAny<int>(), WithAny<string>())).Twice();
	}

	[Fact]
	public async Task WithCustomDelegateWithRefAndOut_SetupShouldWork()
	{
		Mock<DoSomethingWithRefAndOut> mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;
		mock.Setup.Delegate(WithAny<int>(), Ref<int>(v => v + 1), Out(() => 10));

		mock.Subject(1, ref value, out int value2);

		await That(mock.Verify.Invoked(WithAny<int>(), Ref<int>(), Out<int>())).Once();
		await That(value).IsEqualTo(6);
		await That(value2).IsEqualTo(10);
	}

	[Fact]
	public async Task WithCustomDelegateWithRefAndOut_VerifyShouldWork()
	{
		Mock<DoSomethingWithRefAndOut> mock = Mock.Create<DoSomethingWithRefAndOut>();
		int value = 5;

		mock.Subject(1, ref value, out int value2);

		await That(mock.Verify.Invoked(WithAny<int>(), Ref<int>(), Out<int>())).Once();
		await That(value).IsEqualTo(5);
	}

	[Fact]
	public async Task WithCustomGenericDelegate_SetupShouldWork()
	{
		Mock<DoGeneric<long, string>> mock = Mock.Create<DoGeneric<long, string>>();
		mock.Setup.Delegate(WithAny<long>(), WithAny<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		int result1 = mock.Subject(1L, "foo");
		await That(() => mock.Subject(2L, "foo")).Throws<Exception>().WithMessage("foobar");
		int result3 = mock.Subject(2L, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Fact]
	public async Task WithCustomGenericDelegate_VerifyShouldWork()
	{
		Mock<DoGeneric<short, string>> mock = Mock.Create<DoGeneric<short, string>>();

		_ = mock.Subject(1, "foo");
		_ = mock.Subject(2, "bar");

		await That(mock.Verify.Invoked(WithAny<short>(), WithAny<string>())).Twice();
	}

	internal delegate int DoSomething(int x, string y);

	internal delegate void DoSomethingWithRefAndOut(int x, ref int y, out int z);

	internal delegate int DoGeneric<T1, T2>(T1 x, T2 y)
		where T1 : struct
		where T2 : class;
}
