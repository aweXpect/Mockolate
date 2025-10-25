using Mockolate.Monitor;
using Mockolate.Verify;

namespace Mockolate.Tests.Monitor;

public sealed class MockMonitorExtensionsTests
{
	[Fact]
	public async Task DisposeTwice_ShouldNotIncludeMoreInvocations()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		var disposable = mock.Monitor(out var monitor);
		mock.Subject.IsValid(3);
		mock.Subject.IsValid(4);
		disposable.Dispose();
		mock.Subject.IsValid(5);
		mock.Subject.IsValid(6);
		disposable.Dispose();
		mock.Subject.IsValid(7);
		mock.Subject.IsValid(8);

		await That(monitor.Verify.Invoked.IsValid(1)).Never();
		await That(monitor.Verify.Invoked.IsValid(2)).Never();
		await That(monitor.Verify.Invoked.IsValid(3)).Once();
		await That(monitor.Verify.Invoked.IsValid(4)).Once();
		await That(monitor.Verify.Invoked.IsValid(5)).Never();
		await That(monitor.Verify.Invoked.IsValid(6)).Never();
		await That(monitor.Verify.Invoked.IsValid(7)).Never();
		await That(monitor.Verify.Invoked.IsValid(8)).Never();
	}

	[Fact]
	public async Task WhenMonitoringIsNotDisposed_ShouldStillVerify()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		var disposable = mock.Monitor(out var monitor);
		mock.Subject.IsValid(3);
		mock.Subject.IsValid(4);

		await That(monitor.Verify.Invoked.IsValid(1)).Never();
		await That(monitor.Verify.Invoked.IsValid(2)).Never();
		await That(monitor.Verify.Invoked.IsValid(3)).Once();
		mock.Subject.IsValid(5);
		await That(monitor.Verify.Invoked.IsValid(4)).Once();
		await That(monitor.Verify.Invoked.IsValid(5)).Once();
		await That(monitor.Verify.Invoked.IsValid(6)).Never();
	}

	public interface IMyService
	{
		bool IsValid(int id);
	}
}
