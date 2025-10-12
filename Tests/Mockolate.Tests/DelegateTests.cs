using Mockolate.Verify;

namespace Mockolate.Tests;

public class DelegateTests
{
	[Fact]
	public void Action_ShouldBeTreatedAsVoidDelegate()
	{
		bool isCalled = false;
		var mock = Mock.Create<Action>();
		mock.Setup.Delegate().Callback(() => isCalled = true);

		mock.Subject.Invoke();

		mock.Verify.Invoked().Once();
		Assert.True(isCalled);
	}

	[Fact]
	public void Func_ShouldBeTreatedAsReturnDelegate()
	{
		bool isCalled = false;
		var mock = Mock.Create<Func<int>>();
		mock.Setup.Delegate().Callback(() => isCalled = true).Returns(3);

		var result = mock.Subject();

		mock.Verify.Invoked().Once();
		Assert.True(isCalled);
		Assert.Equal(3, result);
	}

	[Fact]
	public async Task WithCustomDelegate_SetupShouldWork()
	{
		var mock = Mock.Create<DoSomething>();
		mock.Setup.Delegate(With.Any<int>(), With.Any<string>())
			.Returns(1)
			.Throws(new Exception("foobar"))
			.Returns(3);

		var result1 = mock.Subject(1, "foo");
		await That(() => mock.Subject(2, "foo")).Throws<Exception>().WithMessage("foobar");
		var result3 = mock.Subject(2, "bar");

		await That(result1).IsEqualTo(1);
		await That(result3).IsEqualTo(3);
	}

	[Fact]
	public async Task WithCustomDelegate_VerifyShouldWork()
	{
		var mock = Mock.Create<DoSomething>();

		_ = mock.Subject(1, "foo");
		_ = mock.Subject(2, "bar");

		mock.Verify.Invoked(With.Any<int>(), With.Any<string>()).Twice();
	}

	internal delegate int DoSomething(int x, string y);
}
