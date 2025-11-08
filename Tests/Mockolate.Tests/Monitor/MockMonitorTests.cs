using Mockolate.Monitor;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Monitor;

public sealed class MockMonitorTests
{
	[Fact]
	public async Task DisposeTwice_ShouldNotIncludeMoreInvocations()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = new(mock);

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		IDisposable disposable = monitor.Run();
		mock.Subject.IsValid(3);
		mock.Subject.IsValid(4);
		disposable.Dispose();
		mock.Subject.IsValid(5);
		mock.Subject.IsValid(6);
		disposable.Dispose();
		mock.Subject.IsValid(7);
		mock.Subject.IsValid(8);

		await That(monitor.Verify.Invoked.IsValid(With(1))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(2))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(3))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(4))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(5))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(6))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(7))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(8))).Never();
	}

	[Fact]
	public async Task MultipleRun_ShouldMonitorInvocationsDuringTheRun()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = new(mock);

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		using (monitor.Run())
		{
			mock.Subject.IsValid(3);
			mock.Subject.IsValid(4);
		}

		mock.Subject.IsValid(5);
		mock.Subject.IsValid(6);
		using (monitor.Run())
		{
			mock.Subject.IsValid(7);
			mock.Subject.IsValid(8);
		}

		mock.Subject.IsValid(9);
		mock.Subject.IsValid(10);

		await That(monitor.Verify.Invoked.IsValid(With(1))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(2))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(3))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(4))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(5))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(6))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(7))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(8))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(9))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(10))).Never();
	}

	[Fact]
	public async Task NestedRun_ShouldThrowInvalidOperationException()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = new(mock);

		void Act()
			=> monitor.Run();

		IDisposable outerRun = monitor.Run();

		await That(Act).Throws<InvalidOperationException>()
			.WithMessage("Monitoring is already running. Dispose the previous scope before starting a new one.");

		outerRun.Dispose();

		await That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task Run_ShouldMonitorInvocationsDuringTheRun()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = new(mock);

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		using (monitor.Run())
		{
			mock.Subject.IsValid(3);
			mock.Subject.IsValid(4);
		}

		mock.Subject.IsValid(5);

		await That(monitor.Verify.Invoked.IsValid(With(1))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(2))).Never();
		await That(monitor.Verify.Invoked.IsValid(With(3))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(4))).Once();
		await That(monitor.Verify.Invoked.IsValid(With(5))).Never();
		await That(mock.Verify.Invoked.IsValid(With(1))).Once();
		await That(mock.Verify.Invoked.IsValid(With(2))).Once();
		await That(mock.Verify.Invoked.IsValid(With(3))).Once();
		await That(mock.Verify.Invoked.IsValid(With(4))).Once();
		await That(mock.Verify.Invoked.IsValid(With(5))).Once();
	}
}
