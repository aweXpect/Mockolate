using Mockolate.Monitor;
using Mockolate.Verify;

namespace Mockolate.Tests.Monitor;

public sealed class MockMonitorTests
{
	[Fact]
	public async Task DisposeTwice_ShouldNotIncludeMoreInvocations()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = mock.Monitor();

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

		monitor.Verify.Invoked.IsValid(1).Never();
		monitor.Verify.Invoked.IsValid(2).Never();
		monitor.Verify.Invoked.IsValid(3).Once();
		monitor.Verify.Invoked.IsValid(4).Once();
		monitor.Verify.Invoked.IsValid(5).Never();
		monitor.Verify.Invoked.IsValid(6).Never();
		monitor.Verify.Invoked.IsValid(7).Never();
		monitor.Verify.Invoked.IsValid(8).Never();
	}

	[Fact]
	public async Task MultipleRun_ShouldMonitorInvocationsDuringTheRun()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = mock.Monitor();

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

		monitor.Verify.Invoked.IsValid(1).Never();
		monitor.Verify.Invoked.IsValid(2).Never();
		monitor.Verify.Invoked.IsValid(3).Once();
		monitor.Verify.Invoked.IsValid(4).Once();
		monitor.Verify.Invoked.IsValid(5).Never();
		monitor.Verify.Invoked.IsValid(6).Never();
		monitor.Verify.Invoked.IsValid(7).Once();
		monitor.Verify.Invoked.IsValid(8).Once();
		monitor.Verify.Invoked.IsValid(9).Never();
		monitor.Verify.Invoked.IsValid(10).Never();
	}

	[Fact]
	public async Task NestedRun_ShouldThrowInvalidOperationException()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		MockMonitor<IMyService, Mock<IMyService>> monitor = mock.Monitor();

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
		MockMonitor<IMyService, Mock<IMyService>> monitor = mock.Monitor();

		mock.Subject.IsValid(1);
		mock.Subject.IsValid(2);
		using (monitor.Run())
		{
			mock.Subject.IsValid(3);
			mock.Subject.IsValid(4);
		}

		mock.Subject.IsValid(5);

		monitor.Verify.Invoked.IsValid(1).Never();
		monitor.Verify.Invoked.IsValid(2).Never();
		monitor.Verify.Invoked.IsValid(3).Once();
		monitor.Verify.Invoked.IsValid(4).Once();
		monitor.Verify.Invoked.IsValid(5).Never();
		mock.Verify.Invoked.IsValid(1).Once();
		mock.Verify.Invoked.IsValid(2).Once();
		mock.Verify.Invoked.IsValid(3).Once();
		mock.Verify.Invoked.IsValid(4).Once();
		mock.Verify.Invoked.IsValid(5).Once();
	}

	public interface IMyService
	{
		bool IsValid(int id);
	}
}
