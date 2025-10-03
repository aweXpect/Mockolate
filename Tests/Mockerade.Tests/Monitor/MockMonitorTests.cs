using Mockerade.Monitor;

namespace Mockerade.Tests.Monitor;

public sealed class MockMonitorTests
{
	[Fact]
	public async Task Run_ShouldMonitorInvocationsDuringTheRun()
	{
		var mock = Mock.For<IMyService>();
		var monitor = mock.Monitor();

		mock.Object.IsValid(1);
		mock.Object.IsValid(2);
		using (monitor.Run())
		{
			mock.Object.IsValid(3);
			mock.Object.IsValid(4);
		}
		mock.Object.IsValid(5);

		await That(monitor.Invoked.IsValid(1).Never());
		await That(monitor.Invoked.IsValid(2).Never());
		await That(monitor.Invoked.IsValid(3).Once());
		await That(monitor.Invoked.IsValid(4).Once());
		await That(monitor.Invoked.IsValid(5).Never());
		await That(mock.Invoked.IsValid(1).Once());
		await That(mock.Invoked.IsValid(2).Once());
		await That(mock.Invoked.IsValid(3).Once());
		await That(mock.Invoked.IsValid(4).Once());
		await That(mock.Invoked.IsValid(5).Once());
	}

	[Fact]
	public async Task MultipleRun_ShouldMonitorInvocationsDuringTheRun()
	{
		var mock = Mock.For<IMyService>();
		var monitor = mock.Monitor();

		mock.Object.IsValid(1);
		mock.Object.IsValid(2);
		using (monitor.Run())
		{
			mock.Object.IsValid(3);
			mock.Object.IsValid(4);
		}
		mock.Object.IsValid(5);
		mock.Object.IsValid(6);
		using (monitor.Run())
		{
			mock.Object.IsValid(7);
			mock.Object.IsValid(8);
		}
		mock.Object.IsValid(9);
		mock.Object.IsValid(10);

		await That(monitor.Invoked.IsValid(1).Never());
		await That(monitor.Invoked.IsValid(2).Never());
		await That(monitor.Invoked.IsValid(3).Once());
		await That(monitor.Invoked.IsValid(4).Once());
		await That(monitor.Invoked.IsValid(5).Never());
		await That(monitor.Invoked.IsValid(6).Never());
		await That(monitor.Invoked.IsValid(7).Once());
		await That(monitor.Invoked.IsValid(8).Once());
		await That(monitor.Invoked.IsValid(9).Never());
		await That(monitor.Invoked.IsValid(10).Never());
	}

	[Fact]
	public async Task NestedRun_ShouldThrowInvalidOperationException()
	{
		var mock = Mock.For<IMyService>();
		var monitor = mock.Monitor();

		void Act()
			=> monitor.Run();

		var outerRun = monitor.Run();

		await That(Act).Throws<InvalidOperationException>()
			.WithMessage("Monitoring is already running. Dispose the previous scope before starting a new one.");

		outerRun.Dispose();

		await That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task DisposeTwice_ShouldNotIncludeMoreInvocations()
	{
		var mock = Mock.For<IMyService>();
		var monitor = mock.Monitor();

		mock.Object.IsValid(1);
		mock.Object.IsValid(2);
		var disposable = monitor.Run();
		mock.Object.IsValid(3);
		mock.Object.IsValid(4);
		disposable.Dispose();
		mock.Object.IsValid(5);
		mock.Object.IsValid(6);
		disposable.Dispose();
		mock.Object.IsValid(7);
		mock.Object.IsValid(8);

		await That(monitor.Invoked.IsValid(1).Never());
		await That(monitor.Invoked.IsValid(2).Never());
		await That(monitor.Invoked.IsValid(3).Once());
		await That(monitor.Invoked.IsValid(4).Once());
		await That(monitor.Invoked.IsValid(5).Never());
		await That(monitor.Invoked.IsValid(6).Never());
		await That(monitor.Invoked.IsValid(7).Never());
		await That(monitor.Invoked.IsValid(8).Never());
	}

	public interface IMyService
	{
		bool IsValid(int id);
	}
}
