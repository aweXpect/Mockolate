using Mockolate.Monitor;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Monitor;

public sealed class MockMonitorExtensionsTests
{
	[Fact]
	public async Task DisposeTwice_ShouldNotIncludeMoreInvocations()
	{
		IMyService sut = Mock.Create<IMyService>();

		sut.IsValid(1);
		sut.IsValid(2);
		IDisposable disposable = sut.MonitorMock(out MockMonitor<IMyService> monitor);
		sut.IsValid(3);
		sut.IsValid(4);
		disposable.Dispose();
		sut.IsValid(5);
		sut.IsValid(6);
		disposable.Dispose();
		sut.IsValid(7);
		sut.IsValid(8);

		await That(monitor.Verify.Invoked.IsValid(It.Is(1))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(2))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(3))).Once();
		await That(monitor.Verify.Invoked.IsValid(It.Is(4))).Once();
		await That(monitor.Verify.Invoked.IsValid(It.Is(5))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(6))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(7))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(8))).Never();
	}

	[Fact]
	public async Task WhenMonitoringIsNotDisposed_ShouldStillVerify()
	{
		IMyService sut = Mock.Create<IMyService>();

		sut.IsValid(1);
		sut.IsValid(2);
		using IDisposable disposable = sut.MonitorMock(out MockMonitor<IMyService> monitor);
		sut.IsValid(3);
		sut.IsValid(4);

		await That(monitor.Verify.Invoked.IsValid(It.Is(1))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(2))).Never();
		await That(monitor.Verify.Invoked.IsValid(It.Is(3))).Once();
		sut.IsValid(5);
		await That(monitor.Verify.Invoked.IsValid(It.Is(4))).Once();
		await That(monitor.Verify.Invoked.IsValid(It.Is(5))).Once();
		await That(monitor.Verify.Invoked.IsValid(It.Is(6))).Never();
	}
}
